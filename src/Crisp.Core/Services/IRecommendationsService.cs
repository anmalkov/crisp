﻿using Crisp.Core.Models;

namespace Crisp.Core.Services;

public interface IRecommendationsService
{
    Task<IEnumerable<string>> GetResourcesAsync();
    Task<Category> GetRecommendationsAsync(IEnumerable<string> resources);
}