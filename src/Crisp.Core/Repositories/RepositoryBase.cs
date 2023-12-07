using Crisp.Core.Models;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Crisp.Core.Repositories;

public class RepositoryBase<T> where T : class, IStorableItem
{
    private const string RepositoriesDirectoryName = "data";

    private readonly string _repositoryFullFilename;

    private ConcurrentDictionary<string, T> _items = new();

    private bool isLoaded = false;


    public RepositoryBase(string repositoryFilename)
    {
        var repositoriesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", RepositoriesDirectoryName);
        _repositoryFullFilename = Path.Combine(repositoriesPath, repositoryFilename);
    }


    public async Task<IEnumerable<T>?> GetAllAsync()
    {
        await LoadAsync();
        return _items.Values;
    }

    public async Task<T?> GetAsync(string id)
    {
        await LoadAsync();
        return _items is not null && _items.TryGetValue(id, out var value) ? value : null;
    }

    public async Task UpdateAllAsync(IEnumerable<T> items)
    {
        _items = new ConcurrentDictionary<string, T>(items.ToDictionary(i => i.Id, i => i));
        await SaveAsync();
    }

    public async Task CreateAsync(T item)
    {
        await LoadAsync();
        _items.TryAdd(item.Id, item);
        await SaveAsync();
    }

    public async Task UpdateAsync(T item)
    {
        await LoadAsync();

        if (_items.ContainsKey(item.Id))
        {
            _items.TryRemove(item.Id, out _);
            _items.TryAdd(item.Id, item);
        }

        await SaveAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await LoadAsync();
        if (!_items.ContainsKey(id))
        {
            return;
        }

        _items.TryRemove(id, out _);
        await SaveAsync();
    }


    private async Task LoadAsync()
    {
        if (!File.Exists(_repositoryFullFilename) || isLoaded)
        {
            return;
        }

        var json = await File.ReadAllTextAsync(_repositoryFullFilename);
        _items = JsonSerializer.Deserialize<ConcurrentDictionary<string, T>>(json) ?? new();
        isLoaded = true;
    }

    private async Task SaveAsync()
    {
        var directoryName = Path.GetDirectoryName(_repositoryFullFilename);
        if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        var json = JsonSerializer.Serialize(_items);
        await File.WriteAllTextAsync(_repositoryFullFilename, json.ToString());
    }
}
