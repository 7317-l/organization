using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PartySchoolApi.Data;
using PartySchoolApi.Helpers;
using PartySchoolApi.Middleware;
using PartySchoolApi.Services.Implementations;
using PartySchoolApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ===== 控制器 =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // 兼容前端以字符串传枚举（role / contentType / questionType 等），序列化仍输出数字
        options.JsonSerializerOptions.Converters.Add(
            new PartySchoolApi.Helpers.FlexibleEnumJsonConverterFactory());
    });

// ===== MySQL数据库 =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ===== JWT认证 =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("SystemAdmin"));
    options.AddPolicy("RequireAdminOrSecretary",
        policy => policy.RequireRole("SystemAdmin", "BranchSecretary"));
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "党校学习双端系统 API",
        Version = "v1",
        Description = "ASP.NET Core Web API 后端接口文档"
    });

    // JWT授权输入框
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请输入JWT令牌，格式：Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== AutoMapper =====
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ===== 依赖注入 =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<JwtHelper>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPartyMemberService, PartyMemberService>();
builder.Services.AddScoped<ILearningContentService, LearningContentService>();
builder.Services.AddScoped<ILearningTaskService, LearningTaskService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IAiService, AiService>();
// ===== 新增服务注册 =====
builder.Services.AddScoped<IPartyDevelopmentService, PartyDevelopmentService>();
builder.Services.AddScoped<IMeetingActivityService, MeetingActivityService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IPointService, PointService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IQwenService, QwenService>();
builder.Services.AddScoped<IKnowledgeSearchService, KnowledgeSearchService>();
builder.Services.AddScoped<IAiKnowledgeService, AiKnowledgeService>();
builder.Services.AddScoped<INl2SqlService, Nl2SqlService>();
builder.Services.AddScoped<IAiContentGenerationService, AiContentGenerationService>();
builder.Services.AddScoped<IAntiCheatService, AntiCheatService>();
builder.Services.AddScoped<IKMeansService, KMeansService>();

var app = builder.Build();

// ===== 全局异常处理中间件 =====
app.UseMiddleware<ExceptionMiddleware>();

// ===== Swagger =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "党校学习系统 API v1");
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
