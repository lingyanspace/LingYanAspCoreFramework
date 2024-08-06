using LingYan.Extension;
using LingYan.Extension.Events;
using LingYan.Model;
using LingYan.Model.BodyModel;
using LingYan.Model.CommonModel;
using LingYan.Model.RequirementModel;
using LingYan.MultiTenant.SysExtension;
using LingYan.MultiTenant.SysMiddlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Extensions.Logging;
using System;
using System.Text;

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
                Console.WriteLine("初始化LYExpose");
                LYExpose.Init();
                Console.WriteLine("初始化LYExpose完成，开始配置Configuration");
                LYExpose.LYBuilderRuntimeManager.ConfigurationManager = builder.GetPropertyValue<ConfigurationManager>("Configuration");
                LYExpose.Config();
                Console.WriteLine("获取项目服务类，并归纳到运行管理");
                LYExpose.LYBuilderRuntimeManager.GainModuleServiceCollectionInit();
                Console.WriteLine("注册模块");
                builder.RegisterModule(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("获取容器");
                var BuilderService = builder.GetBuilderServiceCollection();
                //注册框架自带
                if (builder is WebApplicationBuilder web)
                {

                    web.RegisterProjectSelf();
                }
                Console.WriteLine("注册仓储与工作单元");
                BuilderService.RegisterRepository(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册Manager");
                BuilderService.RegisterManager(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册服务");
                BuilderService.RegisterTService(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册本地事件总线");
                BuilderService.AddSingleton<ILYEventBus, LYEventBus>();
                Console.WriteLine("注册实例");
                BuilderService.RegisterTInstance(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册数据库上下文");
                BuilderService.RegisterDbContext(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("初始化雪花ID");
                BuilderService.RegisterYitIdHelper(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册Redis");
                BuilderService.RegisterRedis(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册动态路由");
                BuilderService.RegisterDynamicWebApi(LYExpose.LYBuilderRuntimeManager);
                Console.WriteLine("注册配置表单大小限制");
                BuilderService.RegisterFormSize();
                Console.WriteLine("注册多租户服务");
                BuilderService.RegisterTenantService();
                Console.WriteLine("注册多租户模版数据上下文");
                BuilderService.RegisterTenantTemplateDbContext(LYExpose.LYBuilderRuntimeManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 初始化项目模块+附带初始化服务
        /// </summary>
        public static void AppInitializationProjectModule(this object app)
        {
            try
            {
                if (app is WebApplication web)
                {
                    web.InitProjectSelf();
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 容器注入框架服务
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterProjectSelf(this WebApplicationBuilder builder)
        {
            Console.WriteLine("日志配置");
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog(LYExpose.NlogConfig);
            Console.WriteLine("添加Endpoint API Explorer，用于展示和测试API端点");
            builder.Services.AddEndpointsApiExplorer();
            Console.WriteLine("添加SignalR，用于实现实时通信功能");
            builder.Services.AddSignalR();
            Console.WriteLine("添加本地MemoryCache缓存，以提供内存中的缓存支持");
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            Console.WriteLine("添加跨域策略");
            if (LYExpose.CrossDomains != null)
            {
                builder.Services.AddCors(c => c.AddPolicy(LYExpose.CrossPolicy, p => p.WithOrigins(LYExpose.CrossDomains).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
            }
            Console.WriteLine("添加安全定义");
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
            Console.WriteLine("注册全局过滤器");
            builder.Services.AddControllers(mvcOption =>
            {
                //添加全局过滤器
                LYExpose.LYBuilderRuntimeManager.ModuleFiler.ForEach(filer =>
                {
                    mvcOption.Filters.Add(filer);
                });
            });
            Console.WriteLine("注册JWT鉴权");
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
            builder.Services.AddHttpContextAccessor();
            //注册授权策略
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

            //1. * *配置Swagger中间件和可视化界面 * *
            app.UseSwagger();
            app.UseSwaggerUI();
            //2. * *配置开发环境异常页面 * *

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            //5. * *配置路由中间件 * *
            app.UseRouting();

            //6. * *配置HTTPS重定向中间件 * *
            app.UseHttpsRedirection();

            //7. * *配置跨域策略 * *
            app.UseCors(LYExpose.CrossPolicy);

            //8. * *配置鉴权中间件 * *
            app.UseAuthentication();

            //9. * *配置授权中间件 * *
            app.UseAuthorization();

            //10. * *模块初始化 * *
            LYExpose.LYBuilderRuntimeManager.ModuleList.InitializationModule(app);

            //3. * *配置静态权限文件夹 * *

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
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LYExpose.HostPhysicsRoot.GetLocalPath(LYExpose.CommonConfigModel.SoftConfig)),
                RequestPath = $"/{LYExpose.CommonConfigModel.SoftConfig}"
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

            //11. * *启用多租户中间件 * *
            app.UseMiddleware<TenantSelectMiddleware>();

            //12. * *初始化多租户配置 * *
            app.Services.InitTenant(LYExpose.LYBuilderRuntimeManager);

            //13. * *配置全局开启身份验证 * *
            app.MapControllers().RequireAuthorization();

            //14. * *配置服务启动监听端口 * *
            LYExpose.ListeningPorts.ToList().ForEach(port =>
            {
                app.Urls.Add(port);
            });

        }
    }
}
