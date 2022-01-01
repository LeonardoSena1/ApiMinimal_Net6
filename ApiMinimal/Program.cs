using ApiMinimal.Data;
using ApiMinimal.ViewModels;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

#region Security

app.UseHsts(options => options.MaxAge(365).IncludeSubdomains().Preload());
app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
app.UseXfo(opts => opts.SameOrigin());
app.UseCsp(opts => opts.BlockAllMixedContent()
                       .StyleSources(s => s.Self())
                       .StyleSources(s => s.Self())
                       .FontSources(s => s.Self())
                       .FormActions(s => s.Self())
                       .FrameAncestors(s => s.Self())
                       .ImageSources(s => s.Self())
                       .ScriptSources(s => s.Self()));
app.UseXContentTypeOptions();
app.UseReferrerPolicy(opts => opts.NoReferrer());
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    await next();
});

#endregion


app.MapGet("v1/todos", (AppDbContext context) =>
{
    var todos = context.Todos.ToList();
    return Results.Ok(todos);
}).Produces<Todo>();


app.MapPost("v1/todos", (AppDbContext context, CreateTodoViewModel model) =>
{
    var todo = model.MapTo();

    if (!model.IsValid)
        return Results.BadRequest(model.Notifications);

    context.Todos.Add(todo);
    context.SaveChanges();

    return Results.Created("", todo);
});


app.Run();
