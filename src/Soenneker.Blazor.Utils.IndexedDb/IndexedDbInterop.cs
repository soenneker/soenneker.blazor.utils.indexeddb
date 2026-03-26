using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Asyncs.Initializers;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Extensions.CancellationTokens;
using Soenneker.Utils.CancellationScopes;
using Soenneker.Blazor.Utils.IndexedDb.Abstract;
using Soenneker.Extensions.String;

namespace Soenneker.Blazor.Utils.IndexedDb;

/// <inheritdoc cref="IIndexedDbInterop"/>
public sealed class IndexedDbInterop : IIndexedDbInterop
{
    private const string _modulePath = "Soenneker.Blazor.Utils.IndexedDb/js/indexeddbinterop.js";
    private const string _jsInitialize = "IndexedDbInterop.initialize";
    private const string _jsEnsureStore = "IndexedDbInterop.ensureStore";
    private const string _jsGet = "IndexedDbInterop.get";
    private const string _jsGetAll = "IndexedDbInterop.getAll";
    private const string _jsSet = "IndexedDbInterop.set";
    private const string _jsRemove = "IndexedDbInterop.remove";
    private const string _jsClear = "IndexedDbInterop.clear";
    private const string _jsContainsKey = "IndexedDbInterop.containsKey";
    private const string _jsGetKeys = "IndexedDbInterop.getKeys";
    private const string _jsGetLength = "IndexedDbInterop.getLength";
    private const string _jsDeleteDatabase = "IndexedDbInterop.deleteDatabase";

    private readonly IJSRuntime _jsRuntime;
    private readonly IResourceLoader _resourceLoader;
    private readonly AsyncInitializer _initializer;
    private readonly CancellationScope _cancellationScope = new();

    private bool _disposed;

    public IndexedDbInterop(IJSRuntime jsRuntime, IResourceLoader resourceLoader)
    {
        _jsRuntime = jsRuntime;
        _resourceLoader = resourceLoader;
        _initializer = new AsyncInitializer(InitializeModule);
    }

    private async ValueTask InitializeModule(CancellationToken cancellationToken)
    {
        _ = await _resourceLoader.ImportModule(_modulePath, cancellationToken);
    }

    private async ValueTask EnsureInitialized(CancellationToken cancellationToken)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await _initializer.Init(linked);
        }
    }

    public async ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsInitialize, linked);
        }
    }

    public async ValueTask EnsureStore(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsEnsureStore, linked, databaseName, storeName);
        }
    }

    public async ValueTask<string?> Get(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(key))
            return null;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            return await _jsRuntime.InvokeAsync<string?>(_jsGet, linked, databaseName, storeName, key);
        }
    }

    public async ValueTask<IReadOnlyList<string>> GetAll(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName))
            return [];

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            var values = await _jsRuntime.InvokeAsync<List<string>>(_jsGetAll, linked, databaseName, storeName);
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
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsSet, linked, databaseName, storeName, key, value ?? "");
        }
    }

    public async ValueTask Remove(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || string.IsNullOrWhiteSpace(storeName) || string.IsNullOrWhiteSpace(key))
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsRemove, linked, databaseName, storeName, key);
        }
    }

    public async ValueTask Clear(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace())
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsClear, linked, databaseName, storeName);
        }
    }

    public async ValueTask<bool> ContainsKey(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
            return false;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            return await _jsRuntime.InvokeAsync<bool>(_jsContainsKey, linked, databaseName, storeName, key);
        }
    }

    public async ValueTask<IReadOnlyList<string>> GetKeys(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace() || storeName.IsNullOrWhiteSpace())
            return [];

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            var keys = await _jsRuntime.InvokeAsync<List<string>>(_jsGetKeys, linked, databaseName, storeName);
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
            await EnsureInitialized(linked);
            return await _jsRuntime.InvokeAsync<int>(_jsGetLength, linked, databaseName, storeName);
        }
    }

    public async ValueTask DeleteDatabase(string databaseName, CancellationToken cancellationToken = default)
    {
        if (databaseName.IsNullOrWhiteSpace())
            return;

        CancellationToken linked = _cancellationScope.CancellationToken.Link(cancellationToken, out CancellationTokenSource? source);

        using (source)
        {
            await EnsureInitialized(linked);
            await _jsRuntime.InvokeVoidAsync(_jsDeleteDatabase, linked, databaseName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await _resourceLoader.DisposeModule(_modulePath);
        await _initializer.DisposeAsync();
        await _cancellationScope.DisposeAsync();
    }
}
