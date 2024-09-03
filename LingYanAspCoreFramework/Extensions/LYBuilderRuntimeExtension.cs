using IGeekFan.AspNetCore.Knife4jUI;
using LingYan.Extension;
using LingYan.Extension.Events;
using LingYan.Model;
using LingYan.Model.BodyModel;
using LingYan.Model.RequirementModel;
using LingYan.MultiTenant.SysExtension;
using LingYan.MultiTenant.SysMiddlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using System.Text;
using static System.Net.WebRequestMethods;

namespace LingYan.DDDModule
{
    public static class LYBuilderRuntimeExtension
    {
        /// <summary>
        /// 注册项目模块+附带注册服务
        /// </summary>
        public static void AppRegisterProjectModule(this object builder)
        {
            try
            {
                 ConsoleColor.DarkYellow.ConsoleLogger("【注入灵燕框架】...");
                LYExpose.Init();
                 ConsoleColor.DarkYellow.ConsoleLogger("【配置系统appsettings.json】...");
                LYExpose.SysConfiguration = builder.GetPropertyValue<ConfigurationManager>("Configuration");
                 ConsoleColor.DarkYellow.ConsoleLogger("【临时归纳项目模块】...");
                LYExpose.LYBuilderRuntimeManager.GainModuleServiceCollectionInit();
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册项目模块】...");
                builder.RegisterModule(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【获取容器】...");
                var BuilderService = builder.GetBuilderServiceCollection();
                ConsoleColor.DarkYellow.ConsoleLogger("【容器注册官方通用服务】...");
                if (builder is WebApplicationBuilder web)
                {
                    web.RegisterProjectSelf();
                }
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册仓储与工作单元】...");
                BuilderService.RegisterRepository(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册领域Manager】...");
                BuilderService.RegisterManager(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册服务TService】...");
                BuilderService.RegisterTService(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册本地事件总线】...");
                BuilderService.AddSingleton<ILYEventBus, LYEventBus>();
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册实例】...");
                BuilderService.RegisterTInstance(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册数据库上下文】...");
                BuilderService.RegisterDbContext(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【初始化雪花ID】...");
                BuilderService.RegisterYitIdHelper(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册Redis】...");
                BuilderService.RegisterRedis(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册动态路由】...");
                BuilderService.RegisterDynamicWebApi(LYExpose.LYBuilderRuntimeManager);
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册配置表单大小限制】...");
                BuilderService.RegisterFormSize();
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册多租户服务】...");
                BuilderService.RegisterTenantService();
                 ConsoleColor.DarkYellow.ConsoleLogger("【注册多租户模版数据上下文】...");
                BuilderService.RegisterTenantTemplateDbContext(LYExpose.LYBuilderRuntimeManager);
            }
            catch (Exception ex)
            {
                ConsoleColor.Red.ConsoleLogger($"【全局注册出错】{ex.Message}...");
            }
        }
        /// <summary>
        /// 初始化项目模块+附带初始化服务
        /// </summary>
        public static void AppInitializationProjectModule(this object app)
        {
            try
            {
                 ConsoleColor.DarkMagenta.ConsoleLogger($"【全局初始化开始】...");
                if (app is WebApplication web)
                {
                    web.InitProjectSelf();
                }
            }
            catch (Exception ex)
            {
                ConsoleColor.Red.ConsoleLogger($"【全局初始化出错】{ex.Message}...");
            }
        }
        /// <summary>
        /// 容器注入框架服务
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterProjectSelf(this WebApplicationBuilder builder)
        {
             ConsoleColor.DarkYellow.ConsoleLogger("【日志清除自带并添加Nlog日志配置】...");
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog(LYExpose.CommonConfigModel.NlogConfig);
             ConsoleColor.DarkYellow.ConsoleLogger("【添加API端点】...");
            builder.Services.AddEndpointsApiExplorer();
             ConsoleColor.DarkYellow.ConsoleLogger("【添加SignalR】...");
            builder.Services.AddSignalR();
             ConsoleColor.DarkYellow.ConsoleLogger("【添加本地MemoryCache缓存】...");
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
             ConsoleColor.DarkYellow.ConsoleLogger("【添加跨域策略】...");
            if (LYExpose.CrossDomains != null)
            {
                builder.Services.AddCors(c => c.AddPolicy(LYExpose.CrossPolicy, p => p.WithOrigins(LYExpose.CrossDomains).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
            }
             ConsoleColor.DarkYellow.ConsoleLogger("【添加安全定义】...");
            builder.Services.AddSwaggerGen(swaggerGenOption =>
            {
                #region 废弃
                // [ApiExplorerSettings(GroupName = "v2")]使用该特性给接口打上可以选择存入哪个接口文档
                //也可以选择IActionModelConvention和IControllerModelConvention
                //文档配置v1
                //swaggerGenOption.SwaggerDoc("v1", new OpenApiInfo()
                //{
                //    Version = "1.0.0.0",
                //    Title = $"项目1.0.0.0",
                //    Description = $"接口文档说明1.0.0.0",
                //    Contact = new OpenApiContact()
                //    {
                //        Name = "框架作者",
                //        Email = "xxx@qq.com",
                //        Url = null
                //    }
                //});
                //文档配置v2
                //swaggerGenOption.SwaggerDoc("v2", new OpenApiInfo()
                //{
                //    Version = "2.0.0.0",
                //    Title = $"项目2.0.0.0",
                //    Description = $"接口文档说明2.0.0.0",
                //    Contact = new OpenApiContact()
                //    {
                //        Name = "框架作者",
                //        Email = "xxx@qq.com",
                //        Url = null
                //    }
                //});
                //注解配置
                //swaggerGenOption.IncludeXmlComments(LYExpose.HostPhysicsRoot.GetLocalUrl("LingYanSpaceWebApi.Api.xml"), true);
                ////接口排序
                //swaggerGenOption.OrderActionsBy(apiDescription => apiDescription.RelativePath.Length.ToString());
                //自定义SchemaId，Swashbuckle中的每个Schema都有唯一的Id，框架会使用这个Id匹配引用类型，因此这个Id不能重复
                //swaggerGenOption.CustomSchemaIds(CustomSchemaIdSelector);
                ////定义JwtBearer认证方式一
                //swaggerGenOption.AddSecurityDefinition(LYExpose.BearerScheme, new OpenApiSecurityScheme()
                //{
                //    Description = "格式:{Token}",
                //    //jwt默认的参数名称
                //    Name = "Authorization",
                //    //jwt默认存放Authorization信息的位置(请求头中)
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.Http,
                //    BearerFormat = "Jwt",
                //    Scheme = "Bearer"
                //});
                ////添加访问前缀
                //swaggerGenOption.AddServer(new OpenApiServer() { Url = "https://localhost:5700", Description = "地址1" });
                //swaggerGenOption.AddServer(new OpenApiServer() { Url = "http://localhost:5800", Description = "地址2" });
                ////如果用代理做虚拟路径时，添加访问前缀
                ////swaggerGenOption.AddServer(new OpenApiServer() { Url = "http://localhost:90/Swashbuckle", Description = "地址1" });
                ////swaggerGenOption.AddServer(new OpenApiServer() { Url = "http://127.0.0.1:90/Swashbuckle", Description = "地址2" });
                //定义JwtBearer认证方式二
                #endregion
                swaggerGenOption.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication1", Version = "v1" });
                swaggerGenOption.AddServer(new OpenApiServer()
                {
                    Url = "",
                    Description = "vvv"
                });
                swaggerGenOption.CustomOperationIds(apiDesc =>
                {
                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    return controllerAction.ControllerName + "-" + controllerAction.ActionName;
                });
                var filePath = Path.Combine(AppContext.BaseDirectory, "WebApplication1.xml");
                swaggerGenOption.IncludeXmlComments(filePath, true);

                swaggerGenOption.AddSecurityDefinition(LYExpose.BearerScheme, new OpenApiSecurityScheme
                {
                    Description = "格式：Bearer {Token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "Jwt",
                    Scheme = "Bearer",
                });
                //安全要求
                swaggerGenOption.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = LYExpose.BearerScheme
                            }
                        },
                        new string[]{ }
                    }
                });
            });
             ConsoleColor.DarkYellow.ConsoleLogger("【注册全局过滤器】...");
            builder.Services.AddControllers(mvcOption =>
            {
                //添加全局过滤器
                LYExpose.LYBuilderRuntimeManager.ModuleFiler.ForEach(filer =>
                {
                    mvcOption.Filters.Add(filer);
                });
            });
             ConsoleColor.DarkYellow.ConsoleLogger("【注册JWT鉴权】...");
            builder.Services.AddAuthentication(options =>
            {
                //自定义鉴权方案可以添加
                options.DefaultScheme = LYExpose.BearerScheme;
            }).AddJwtBearer(LYExpose.BearerScheme, options =>
            {
                try
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        //是否验证Issuer
                        ValidateIssuer = true,
                        //发行人Issuer
                        ValidIssuer = LYExpose.JwtModel.Issuer,
                        //是否验证Audience
                        ValidateAudience = true,
                        //订阅人Audience
                        ValidAudience = LYExpose.JwtModel.Audience,
                        //是否验证SecurityKey
                        ValidateIssuerSigningKey = true,
                        //SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LYExpose.JwtModel.SecretKey)),
                        //密码算法
                        //是否验证失效时间
                        ValidateLifetime = true,
                        //过期时间容错值，解决服务器端时间不同步问题（秒）
                        ClockSkew = TimeSpan.FromDays(Convert.ToDouble(LYExpose.JwtModel.Expres)),
                        RequireExpirationTime = true,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            //终止
                            context.HandleResponse();
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
                            context.Response.WriteAsJsonAsync(new ResponceBody(203, "鉴权失败", null));
                            return Task.FromResult(0);
                        }
                    };
                }
                catch (Exception ex)
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            //终止
                            context.HandleResponse();
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
                            context.Response.WriteAsJsonAsync(new ResponceBody(202, "鉴权参数验证不通过", null));
                            return Task.FromResult(0);
                        }
                    };
                }
            });
             ConsoleColor.DarkYellow.ConsoleLogger("【HTTP 上下文的访问】...");
            builder.Services.AddHttpContextAccessor();
             ConsoleColor.DarkYellow.ConsoleLogger("【注册授权策略】...");
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(LYExpose.EmpowerPolicy, policy =>
                {
                    policy.Requirements.Add(new PermissionRequirement(builder.Services.BuildServiceProvider().CreateScope()));
                });
            });

        }
        private static string CustomSchemaIdSelector(Type modelType)
        {
            if (!modelType.IsConstructedGenericType)
                return modelType.FullName.Replace("[]", "Array");
            var prefix = modelType.GetGenericArguments()
                .Select(genericArg => CustomSchemaIdSelector(genericArg))
                .Aggregate((previous, current) => previous + current);
            return prefix + modelType.FullName.Split('`').First();
        }
        /// <summary>
        /// 容器管理员初始化框架服务
        /// </summary>
        /// <param name="app"></param>
        private static void InitProjectSelf(this WebApplication app)
        {
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置Swagger中间件和可视化界面】...");
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseKnife4UI(c =>
                {
                    c.RoutePrefix = string.Empty;
                    c.SwaggerEndpoint($"/swagger/v1/swagger.json", "h.swagger.webapi v1");
                });
            }
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置开发环境异常页面】...");
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置路由中间件】...");
            app.UseRouting();
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置HTTPS重定向中间件】...");
            app.UseHttpsRedirection();
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置跨域策略】...");
            app.UseCors(LYExpose.CrossPolicy);
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置鉴权中间件】...");
            app.UseAuthentication();
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置授权中间件】...");
            app.UseAuthorization();
             ConsoleColor.DarkMagenta.ConsoleLogger($"【项目各模块初始化】...");
            LYExpose.LYBuilderRuntimeManager.ModuleList.InitializationModule(app);
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置静态权限文件夹】...");
            app.UseWhen(context => context.Request.Path.StartsWithSegments($"/{LYExpose.CommonConfigModel.LimitFile}"), appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new ResponceBody(40000, "用户鉴权不通过", null));
                        return;
                    }
                    await next();
                });
            });
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置静态开放文件夹】...");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LYExpose.HostPhysicsRoot.GetLocalPath(LYExpose.CommonConfigModel.SoftFile)),
                RequestPath = $"/{LYExpose.CommonConfigModel.SoftFile}"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LYExpose.HostPhysicsRoot.GetLocalPath(LYExpose.CommonConfigModel.LimitFile)),
                RequestPath = $"/{LYExpose.CommonConfigModel.LimitFile}"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LYExpose.HostPhysicsRoot.GetLocalPath(LYExpose.CommonConfigModel.OpenFile)),
                RequestPath = $"/{LYExpose.CommonConfigModel.OpenFile}"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LYExpose.HostPhysicsRoot.GetLocalPath(LYExpose.CommonConfigModel.ChatFile)),
                RequestPath = $"/{LYExpose.CommonConfigModel.ChatFile}"
            }); 
             ConsoleColor.DarkMagenta.ConsoleLogger($"【启用多租户中间件】...");
            app.UseMiddleware<TenantSelectMiddleware>();
             ConsoleColor.DarkMagenta.ConsoleLogger($"【初始化多租户配置】...");
            app.Services.InitTenant(LYExpose.LYBuilderRuntimeManager);
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置全局开启身份验证】...");
            app.MapControllers().RequireAuthorization();
             ConsoleColor.DarkMagenta.ConsoleLogger($"【配置服务启动监听端口】...");
            LYExpose.ListeningPorts.ToList().ForEach(port =>
            {
                app.Urls.Add(port);
            });

        }
    }
}
