using AspNetSpaTemplate.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public class EventBusImplementationTests
{
    private readonly Mock<ILogger<EventBusImplementation>> _loggerMock;
    private readonly EventBusImplementation _eventBus;

    public EventBusImplementationTests()
    {
        _loggerMock = new Mock<ILogger<EventBusImplementation>>();
        _eventBus = new EventBusImplementation(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeLogger()
    {
        // Act
        var eventBus = new EventBusImplementation(_loggerMock.Object);

        // Assert
        eventBus.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        Func<Task> act = async () => await _eventBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Publishing event: ProductCreatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task Subscribe_ShouldAddHandlerToSubscribersList()
    {
        // Arrange
        Func<ProductCreatedEvent, Task> handler = _ => Task.CompletedTask;

        // Act
        _eventBus.Subscribe(handler);

        // Assert
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(1);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Subscribed handler to ProductCreatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Subscribe_WithNullHandler_ShouldThrowArgumentNullException()
    {
        // Arrange
        Func<ProductCreatedEvent, Task>? nullHandler = null;

        // Act
        Action act = () => _eventBus.Subscribe(nullHandler!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Unsubscribe_ShouldRemoveHandlerFromSubscribersList()
    {
        // Arrange
        Func<ProductCreatedEvent, Task> handler = _ => Task.CompletedTask;
        _eventBus.Subscribe(handler);

        // Act
        _eventBus.Unsubscribe(handler);

        // Assert
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(0);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Subscribed handler to ProductCreatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public void Unsubscribe_WithNonExistentHandler_ShouldNotThrow()
    {
        // Arrange
        Func<ProductCreatedEvent, Task> handler1 = _ => Task.CompletedTask;
        Func<ProductCreatedEvent, Task> handler2 = _ => Task.CompletedTask;
        _eventBus.Subscribe(handler1);

        // Act
        var act = () => _eventBus.Unsubscribe(handler2);

        // Assert
        act.Should().NotThrow();
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(1);
    }

    [Fact]
    public async Task PublishAsync_WithSingleHandler_ShouldCallHandler()
    {
        // Arrange
        var handlerCalled = false;
        var handlerExecutedEvent = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler = _ => Task.Run(() =>
        {
            handlerCalled = true;
            handlerExecutedEvent.SetResult(true);
        });

        _eventBus.Subscribe(handler);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        await _eventBus.PublishAsync(@event);
        await handlerExecutedEvent.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        handlerCalled.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Event published: ProductCreatedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_ShouldCallAllHandlers()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;
        var allHandlersCalled = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() => handler1Called = true);
        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() => handler2Called = true);
        Func<ProductCreatedEvent, Task> handler3 = _ => Task.Run(() =>
        {
            handler3Called = true;
            if (handler1Called && handler2Called)
            {
                allHandlersCalled.SetResult(true);
            }
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        await _eventBus.PublishAsync(@event);
        await allHandlersCalled.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        handler1Called.Should().BeTrue();
        handler2Called.Should().BeTrue();
        handler3Called.Should().BeTrue();
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(3);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_ShouldCallHandlersInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var orderReceived = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            executionOrder.Add(1);
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            executionOrder.Add(2);
        });

        Func<ProductCreatedEvent, Task> handler3 = _ => Task.Run(() =>
        {
            executionOrder.Add(3);
            if (executionOrder.Count == 3)
            {
                orderReceived.SetResult(true);
            }
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        await _eventBus.PublishAsync(@event);
        await orderReceived.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        executionOrder.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task PublishAsync_WithHandlerThatThrows_ShouldContinueAndNotThrow()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        var allHandlersCalled = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            handler1Called = true;
            throw new InvalidOperationException("Handler 1 failed");
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            handler2Called = true;
            allHandlersCalled.SetResult(true);
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        Func<Task> act = async () => await _eventBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
        handler1Called.Should().BeTrue();
        handler2Called.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error in event handler for ProductCreatedEvent")),
                It.IsAny<AggregateException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithHandlerThatFailsTwiceThenSucceeds_ShouldRetryAndSucceedWithoutDeadLettering()
    {
        // Arrange
        var deadLetterSinkMock = new Mock<IDeadLetterSink>();
        var eventBus = new EventBusImplementation(_loggerMock.Object, deadLetterSinkMock.Object);

        var attemptCount = 0;
        var succeeded = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler = _ => Task.Run(() =>
        {
            attemptCount++;
            if (attemptCount < 3)
                throw new InvalidOperationException($"Attempt {attemptCount} failed");

            succeeded.SetResult(true);
        });

        eventBus.Subscribe(handler);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        await eventBus.PublishAsync(@event);
        await succeeded.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        attemptCount.Should().Be(3);
        deadLetterSinkMock.Verify(
            x => x.SendAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishAsync_WithHandlerThatAlwaysFails_ShouldDeadLetterAfterRetriesAndStillRunOtherHandlers()
    {
        // Arrange
        var deadLetterSinkMock = new Mock<IDeadLetterSink>();
        deadLetterSinkMock
            .Setup(x => x.SendAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var eventBus = new EventBusImplementation(_loggerMock.Object, deadLetterSinkMock.Object);

        var failingAttempts = 0;
        var secondHandlerCalled = false;
        var secondHandlerExecuted = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> failingHandler = _ => Task.Run(() =>
        {
            failingAttempts++;
            throw new InvalidOperationException($"Attempt {failingAttempts} failed");
        });

        Func<ProductCreatedEvent, Task> secondHandler = _ => Task.Run(() =>
        {
            secondHandlerCalled = true;
            secondHandlerExecuted.SetResult(true);
        });

        eventBus.Subscribe(failingHandler);
        eventBus.Subscribe(secondHandler);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        Func<Task> act = async () => await eventBus.PublishAsync(@event);

        // Assert
        await act.Should().NotThrowAsync();
        await secondHandlerExecuted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        failingAttempts.Should().Be(3);
        secondHandlerCalled.Should().BeTrue();
        deadLetterSinkMock.Verify(
            x => x.SendAsync(
                It.Is<ProductCreatedEvent>(e => e.EventId == @event.EventId),
                It.IsAny<AggregateException>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishManyAsync_WithMultipleEvents_ShouldCallHandlersForEachEvent()
    {
        // Arrange
        var handlerCallCount = 0;
        var allEventsProcessed = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler = _ => Task.Run(() =>
        {
            handlerCallCount++;
            if (handlerCallCount == 3)
            {
                allEventsProcessed.SetResult(true);
            }
        });

        _eventBus.Subscribe(handler);

        var events = new List<ProductCreatedEvent>
        {
            new() { ProductId = 1, ProductName = "Product 1", Price = 10.00m },
            new() { ProductId = 2, ProductName = "Product 2", Price = 20.00m },
            new() { ProductId = 3, ProductName = "Product 3", Price = 30.00m }
        };

        // Act
        await _eventBus.PublishManyAsync(events);
        await allEventsProcessed.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        handlerCallCount.Should().Be(3);
    }

    [Fact]
    public void Clear_ShouldRemoveAllSubscribers()
    {
        // Arrange
        Func<ProductCreatedEvent, Task> handler1 = _ => Task.CompletedTask;
        Func<OrderPlacedEvent, Task> handler2 = _ => Task.CompletedTask;

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);

        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(1);
        _eventBus.GetSubscriberCount<OrderPlacedEvent>().Should().Be(1);

        // Act
        _eventBus.Clear();

        // Assert
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(0);
        _eventBus.GetSubscriberCount<OrderPlacedEvent>().Should().Be(0);
    }

    [Fact]
    public async Task GetSubscriberCount_WithNoSubscribers_ShouldReturnZero()
    {
        // Act & Assert
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(0);
        _eventBus.GetSubscriberCount<OrderPlacedEvent>().Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_WithDifferentEventTypes_ShouldOnlyCallRelevantHandlers()
    {
        // Arrange
        var productHandlerCalled = false;
        var orderHandlerCalled = false;
        var allHandlersCalled = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> productHandler = _ => Task.Run(() => productHandlerCalled = true);
        Func<OrderPlacedEvent, Task> orderHandler = _ => Task.Run(() => orderHandlerCalled = true);

        _eventBus.Subscribe(productHandler);
        _eventBus.Subscribe(orderHandler);

        var productEvent = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        await _eventBus.PublishAsync(productEvent);
        await Task.Delay(100); // Give handlers time to execute

        // Assert
        productHandlerCalled.Should().BeTrue();
        orderHandlerCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Unsubscribe_ShouldRemoveOnlySpecificHandler()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        var handler1Executed = new TaskCompletionSource<bool>();
        var handler2Executed = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            handler1Called = true;
            handler1Executed.SetResult(true);
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            handler2Called = true;
            handler2Executed.SetResult(true);
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        await _eventBus.PublishAsync(@event);
        await handler1Executed.Task;
        await handler2Executed.Task;

        handler1Called.Should().BeTrue();
        handler2Called.Should().BeTrue();

        // Act - unsubscribe handler1
        _eventBus.Unsubscribe(handler1);

        // Reset flags
        handler1Called = false;
        handler2Called = false;

        await _eventBus.PublishAsync(@event);
        await Task.Delay(100);

        // Assert
        handler1Called.Should().BeFalse();
        handler2Called.Should().BeTrue();
    }

    [Fact]
    public async Task Unsubscribe_MidDispatch_ShouldNotThrowAndShouldNotAffectCurrentDispatch()
    {
        // Arrange
        var executionOrder = new List<string>();
        var unsubscribeDuringDispatch = new TaskCompletionSource<bool>();
        var dispatchCompleted = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            executionOrder.Add("handler1");
            // Signal that we're in the middle of dispatch
            unsubscribeDuringDispatch.SetResult(true);
            // Wait a bit to ensure unsubscribe happens during dispatch
            Thread.Sleep(50);
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            executionOrder.Add("handler2");
        });

        Func<ProductCreatedEvent, Task> handler3 = _ => Task.Run(() =>
        {
            executionOrder.Add("handler3");
            dispatchCompleted.SetResult(true);
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act - Start publishing and unsubscribe handler2 while dispatch is in progress
        var publishTask = _eventBus.PublishAsync(@event);

        // Wait until we're inside handler1
        await unsubscribeDuringDispatch.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Unsubscribe handler2 while dispatch is still in progress
        // This should not throw and should not affect the current dispatch (handlers list is copied)
        _eventBus.Unsubscribe(handler2);

        // Wait for dispatch to complete
        await dispatchCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await publishTask;

        // Assert - All handlers should execute because PublishAsync copies the handlers list
        executionOrder.Should().BeEquivalentTo(new[] { "handler1", "handler2", "handler3" });
        // After dispatch completes, handler2 should be unsubscribed
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(2); // handler1 and handler3
    }

    [Fact]
    public async Task PublishAsync_FromWithinHandler_ShouldNotDeadlockAndShouldExecuteAllHandlers()
    {
        // Arrange
        var executionOrder = new List<string>();
        var nestedPublishCompleted = new TaskCompletionSource<bool>();
        var outerPublishCompleted = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            executionOrder.Add("handler1-start");

            // Publish another event from within handler (re-entrancy test)
            var nestedEvent = new OrderPlacedEvent
            {
                OrderId = 1,
                UserId = 1,
                TotalAmount = 100.00m,
                ItemCount = 5
            };

            // This should not deadlock or cause issues
            _eventBus.PublishAsync(nestedEvent).GetAwaiter().GetResult();

            executionOrder.Add("handler1-nested-publish-complete");
            nestedPublishCompleted.SetResult(true);
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            executionOrder.Add("handler2");
            outerPublishCompleted.SetResult(true);
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act - Publish event that will trigger nested publish
        await _eventBus.PublishAsync(@event);

        // Wait for nested publish to complete
        await nestedPublishCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await outerPublishCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        // Assert
        executionOrder.Should().Contain("handler1-start");
        executionOrder.Should().Contain("handler1-nested-publish-complete");
        executionOrder.Should().Contain("handler2");
        // Verify both handlers were called
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(2);
    }

    [Fact]
    public async Task PublishAsync_FromWithinHandler_ShouldNotCauseInfiniteLoop()
    {
        // Arrange
        var executionCount = 0;
        var maxExecutionCount = 10; // Safety limit

        Func<ProductCreatedEvent, Task> handler = _ => Task.Run(() =>
        {
            executionCount++;

            // This handler publishes the same event type it handles
            // Should not cause infinite loop due to copy of handlers list during dispatch
            var sameEvent = new ProductCreatedEvent
            {
                ProductId = executionCount,
                ProductName = "Test Product",
                Price = (decimal)executionCount
            };

            // Try to publish from within handler
            _eventBus.PublishAsync(sameEvent).GetAwaiter().GetResult();
        });

        _eventBus.Subscribe(handler);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act
        await _eventBus.PublishAsync(@event);

        // Assert - execution should be limited (not infinite)
        executionCount.Should().BeLessThan(maxExecutionCount);
    }

    [Fact]
    public async Task Unsubscribe_MultipleHandlersMidDispatch_ShouldNotAffectCurrentDispatch()
    {
        // Arrange
        var executionOrder = new List<int>();
        var unsubscribeSignal = new TaskCompletionSource<bool>();
        var dispatchCompleted = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            executionOrder.Add(1);
            unsubscribeSignal.SetResult(true);
            Thread.Sleep(50);
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            executionOrder.Add(2);
        });

        Func<ProductCreatedEvent, Task> handler3 = _ => Task.Run(() =>
        {
            executionOrder.Add(3);
        });

        Func<ProductCreatedEvent, Task> handler4 = _ => Task.Run(() =>
        {
            executionOrder.Add(4);
            dispatchCompleted.SetResult(true);
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);
        _eventBus.Subscribe(handler4);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act - Start publishing and unsubscribe multiple handlers while dispatch is in progress
        var publishTask = _eventBus.PublishAsync(@event);

        // Wait until we're inside handler1
        await unsubscribeSignal.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Unsubscribe multiple handlers while dispatch is still in progress
        // This should not throw and should not affect the current dispatch (handlers list is copied)
        _eventBus.Unsubscribe(handler2);
        _eventBus.Unsubscribe(handler3);

        // Wait for dispatch to complete
        await dispatchCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await publishTask;

        // Assert - All handlers should execute because PublishAsync copies the handlers list
        executionOrder.Should().BeEquivalentTo(new[] { 1, 2, 3, 4 });
        // After dispatch completes, handlers 2 and 3 should be unsubscribed
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(2); // handler1 and handler4
    }

    [Fact]
    public async Task ConcurrentUnsubscribe_ShouldNotCauseRaceConditions()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        var handler3Called = false;
        var completionSource = new TaskCompletionSource<bool>();

        Func<ProductCreatedEvent, Task> handler1 = _ => Task.Run(() =>
        {
            handler1Called = true;
        });

        Func<ProductCreatedEvent, Task> handler2 = _ => Task.Run(() =>
        {
            handler2Called = true;
            completionSource.SetResult(true);
        });

        Func<ProductCreatedEvent, Task> handler3 = _ => Task.Run(() =>
        {
            handler3Called = true;
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        var @event = new ProductCreatedEvent
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 9.99m
        };

        // Act - Publish event
        await _eventBus.PublishAsync(@event);
        await completionSource.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert - All handlers should be called
        handler1Called.Should().BeTrue();
        handler2Called.Should().BeTrue();
        handler3Called.Should().BeTrue();

        // Now perform concurrent unsubscribes
        var unsubscribeTasks = new Task[3];
        unsubscribeTasks[0] = Task.Run(() => _eventBus.Unsubscribe(handler1));
        unsubscribeTasks[1] = Task.Run(() => _eventBus.Unsubscribe(handler2));
        unsubscribeTasks[2] = Task.Run(() => _eventBus.Unsubscribe(handler3));

        await Task.WhenAll(unsubscribeTasks);

        // Should not throw or cause race conditions
        _eventBus.GetSubscriberCount<ProductCreatedEvent>().Should().Be(0);
    }
}