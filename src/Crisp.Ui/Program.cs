using Crisp.Core.Repositories;
using Crisp.Core.Services;
using Crisp.Ui.Extensions;
using Crisp.Ui.Requests;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(config => {
    config.Lifetime = ServiceLifetime.Scoped;
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IGitHubRepository, GitHubGitRepository>();
builder.Services.AddSingleton<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddSingleton<IThreatModelCategoriesRepository, ThreatModelCategoriesRepository>();
builder.Services.AddSingleton<IThreatModelsRepository, ThreatModelsRepository>();
builder.Services.AddSingleton<IReportsRepository, ReportsRepository>();
builder.Services.AddSingleton<ISecurityBenchmarksRepository, SecurityBenchmarksV3Repository>();

builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IThreatModelsService, ThreatModelsService>();
builder.Services.AddScoped<IRecommendationsService, RecommendationsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MediateGet<GetCategoriesRequest>("/api/categories");

app.MediateGet<GetResourcesRequest>("/api/resources");
app.MediatePost<GetRecommendationsRequest>("/api/resources/recommendations");
app.MediateGet<GetBenchmarkControlsRequest>("/api/benchmark/controls");

app.MediateGet<GetThreatModelsRequest>("/api/threatmodels");
app.MediateGet<GetThreatModelCategoriesRequest>("/api/threatmodels/categories");
app.MediatePost<CreateThreatModelRequest>("/api/threatmodels");
app.MediatePut<UpdateThreatModelRequest>("/api/threatmodels/{id}");
app.MediatePost<UploadThreatModelFilesRequest>("/api/threatmodels/{id}/upload");
app.MediateGet<GetThreatModelFileRequest>("/api/threatmodels/{id}/file/{fileName}");
app.MediateGet<GetThreatModelReportRequest>("/api/threatmodels/{id}/report");
app.MediateGet<GetThreatModelReportArchiveRequest>("/api/threatmodels/{id}/report/archive");
app.MediateDelete<DeleteThreatModelRequest>("/api/threatmodels/{id}");

app.MapFallbackToFile("index.html");

app.Run();
