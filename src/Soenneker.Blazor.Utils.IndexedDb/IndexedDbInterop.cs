using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;
using Soenneker.Blazor.Utils.IndexedDb.Abstract;
using Soenneker.Extensions.String;

namespace Soenneker.Blazor.Utils.IndexedDb;

/// <inheritdoc cref="IIndexedDbInterop"/>
public sealed class IndexedDbInterop : IIndexedDbInterop
{
    private const string _modulePath = "/_content/Soenneker.Blazor.Utils.IndexedDb/js/indexeddbinterop.js";

    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly CancellationScope _cancellationScope = new();

    public IndexedDbInterop(IModuleImportUtil moduleImportUtil)
    {
        _moduleImportUtil = moduleImportUtil;
    }

    public async ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("initialize", linked);
        }
    }

    public async ValueTask EnsureStore(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("ensureStore", linked, databaseName, storeName);
        }
    }

    public async ValueTask<string?> Get(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(key))
            return null;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            return await module.InvokeAsync<string?>("get", linked, databaseName, storeName, key);
        }
    }

    public async ValueTask<IReadOnlyList<string>> GetAll(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName))
            return [];

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            var values = await module.InvokeAsync<List<string>>("getAll", linked, databaseName, storeName);
            return values ?? [];
        }
    }

    public async ValueTask Set(string databaseName, string storeName, string key, string value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(key))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("set", linked, databaseName, storeName, key, value ?? "");
        }
    }

    public async ValueTask Remove(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(key))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("remove", linked, databaseName, storeName, key);
        }
    }

    public async ValueTask Clear(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace())
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("clear", linked, databaseName, storeName);
        }
    }

    public async ValueTask<bool> ContainsKey(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
            return false;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            return await module.InvokeAsync<bool>("containsKey", linked, databaseName, storeName, key);
        }
    }

    public async ValueTask<IReadOnlyList<string>> GetKeys(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace())
            return [];

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            var keys = await module.InvokeAsync<List<string>>("getKeys", linked, databaseName, storeName);
            return keys ?? [];
        }
    }

    public async ValueTask<int> GetLength(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace())
            return 0;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            return await module.InvokeAsync<int>("getLength", linked, databaseName, storeName);
        }
    }

    public async ValueTask DeleteDatabase(string databaseName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace())
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, linked);
            await module.InvokeVoidAsync("deleteDatabase", linked, databaseName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _moduleImportUtil.DisposeContentModule(_modulePath);
        await _cancellationScope.DisposeAsync();
    }
}
