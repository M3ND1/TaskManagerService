# MediatR Validation Pipeline - Transport Examples

## How the ValidationBehavior Works

```
Client Request → Transport Layer → MediatR.Send() → ValidationBehavior → Handler
                     ↓                                     ↓
                 (HTTP/gRPC/SignalR)           (Runs FluentValidation)
```

---

## 1. HTTP (What you have now)

**Traditional ASP.NET Core REST API**

```csharp
// Controller (current approach)
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        // Without ValidationBehavior:
        // ✅ FluentValidation runs here (AddFluentValidationAutoValidation)

        var command = new CreateUserCommand(dto);
        var result = await _mediator.Send(command);  // ✅ Also validated in pipeline
        return Ok(result);
    }
}
```

**Client calls it:**

```bash
curl -X POST https://api.example.com/api/users \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "SecurePass123"}'
```

---

## 2. gRPC (Google Remote Procedure Call)

**High-performance binary protocol** - used for microservice-to-microservice communication.

```csharp
// gRPC Service (no controller, no FluentValidation auto-wiring)
public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IMediator _mediator;

    public override async Task<CreateUserResponse> CreateUser(
        CreateUserRequest request,
        ServerCallContext context)
    {
        // Map gRPC request to DTO
        var dto = new CreateUserDto
        {
            Email = request.Email,
            Password = request.Password,
            Username = request.Username
        };

        var command = new CreateUserCommand(dto);

        // ❌ WITHOUT ValidationBehavior: No validation happens!
        // ✅ WITH ValidationBehavior: Validated automatically
        var result = await _mediator.Send(command, context.CancellationToken);

        return new CreateUserResponse { UserId = result.Id };
    }
}
```

**Client calls it:**

```csharp
// Another microservice
var channel = GrpcChannel.ForAddress("https://user-service:5001");
var client = new UserService.UserServiceClient(channel);

var response = await client.CreateUserAsync(new CreateUserRequest
{
    Email = "user@example.com",
    Password = "SecurePass123"
});
```

**Why gRPC?**

- 7x faster than REST (binary protocol, HTTP/2)
- Strongly typed contracts (.proto files)
- Used in microservices, mobile apps (low bandwidth)

---

## 3. SignalR (Real-time WebSockets)

**Real-time bidirectional communication** - server can push to clients.

```csharp
// SignalR Hub (no controller, no HTTP context)
public class UserHub : Hub
{
    private readonly IMediator _mediator;

    public async Task<UserResponseDto> RegisterUser(CreateUserDto dto)
    {
        // Client calls this method directly via WebSocket

        var command = new CreateUserCommand(dto);

        // ❌ WITHOUT ValidationBehavior: No validation!
        // ✅ WITH ValidationBehavior: Validated automatically
        var result = await _mediator.Send(command, Context.ConnectionAborted);

        // Notify all connected clients
        await Clients.All.SendAsync("UserCreated", result);

        return result;
    }
}
```

**Client calls it:**

```javascript
// Browser JavaScript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/userHub")
  .build();

await connection.start();

const newUser = await connection.invoke("RegisterUser", {
  email: "user@example.com",
  password: "SecurePass123",
  username: "johndoe",
});

// Also receive real-time updates
connection.on("UserCreated", (user) => {
  console.log("New user registered:", user);
});
```

**Why SignalR?**

- Chat applications
- Live dashboards (stock prices, sports scores)
- Real-time notifications
- Collaborative editing (think Google Docs)

---

## 4. Background Jobs (Hangfire/Quartz)

**Scheduled or queued tasks** - no HTTP request at all.

```csharp
// Background job that runs every day at midnight
public class UserCleanupJob
{
    private readonly IMediator _mediator;

    [AutomaticRetry(Attempts = 3)]
    public async Task CleanupInactiveUsers()
    {
        // This runs on a timer, no HTTP request

        var inactiveUsers = await GetInactiveUsers();

        foreach (var user in inactiveUsers)
        {
            var command = new DeleteUserCommand(user.Id);

            // ❌ WITHOUT ValidationBehavior: No validation!
            // ✅ WITH ValidationBehavior: Validated automatically
            await _mediator.Send(command);
        }
    }
}
```

**Scheduled via:**

```csharp
// Program.cs
RecurringJob.AddOrUpdate<UserCleanupJob>(
    "cleanup-users",
    job => job.CleanupInactiveUsers(),
    Cron.Daily(0)); // Every day at midnight
```

---

## 5. Message Queue Consumer (RabbitMQ/Azure Service Bus)

**Asynchronous message processing** - messages come from a queue.

```csharp
// Message handler (listens to queue)
public class UserRegistrationConsumer : IConsumer<UserRegistrationMessage>
{
    private readonly IMediator _mediator;

    public async Task Consume(ConsumeContext<UserRegistrationMessage> context)
    {
        var message = context.Message;

        var dto = new CreateUserDto
        {
            Email = message.Email,
            Password = message.Password,
            Username = message.Username
        };

        var command = new CreateUserCommand(dto);

        // ❌ WITHOUT ValidationBehavior: No validation!
        // ✅ WITH ValidationBehavior: Validated automatically
        var result = await _mediator.Send(command);

        await context.Publish(new UserCreatedEvent { UserId = result.Id });
    }
}
```

**Message sent from another service:**

```csharp
// Payment service sends a message after payment succeeds
await bus.Publish(new UserRegistrationMessage
{
    Email = "user@example.com",
    Password = "SecurePass123",
    Username = "johndoe"
});
```

---

## 6. Unit Tests (Direct Handler Testing)

```csharp
[Fact]
public async Task Should_Reject_Invalid_Email()
{
    // Arrange
    var handler = new CreateUserCommandHandler(...);
    var invalidDto = new CreateUserDto
    {
        Email = "not-an-email",  // Invalid!
        Password = "SecurePass123",
        Username = "johndoe"
    };
    var command = new CreateUserCommand(invalidDto);

    // Act & Assert
    // ❌ WITHOUT ValidationBehavior: Handler receives garbage data
    // ✅ WITH ValidationBehavior: Throws ValidationException before handler runs

    await Assert.ThrowsAsync<ValidationException>(
        () => handler.Handle(command, CancellationToken.None)
    );
}
```

---

## Summary: When ValidationBehavior Saves You

| Transport              | Without Behavior                     | With Behavior                     |
| ---------------------- | ------------------------------------ | --------------------------------- |
| **HTTP (Controllers)** | ✅ Validated (FluentValidation auto) | ✅ Validated (redundant but safe) |
| **gRPC**               | ❌ NOT validated                     | ✅ Validated                      |
| **SignalR**            | ❌ NOT validated                     | ✅ Validated                      |
| **Background Jobs**    | ❌ NOT validated                     | ✅ Validated                      |
| **Message Queues**     | ❌ NOT validated                     | ✅ Validated                      |
| **Unit Tests**         | ❌ NOT validated                     | ✅ Validated                      |
| **Handler → Handler**  | ❌ NOT validated                     | ✅ Validated                      |

---

## How to Register It

```csharp
// Program.cs
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MappingProfile).Assembly);

    // Add validation pipeline - runs BEFORE all handlers
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
```

Now **every** MediatR request gets validated, regardless of how it's called.
