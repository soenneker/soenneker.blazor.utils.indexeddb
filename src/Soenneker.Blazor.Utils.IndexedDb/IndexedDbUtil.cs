using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Blazor.Utils.IndexedDb.Abstract;
using Soenneker.Utils.Json;
using Soenneker.Extensions.String;

namespace Soenneker.Blazor.Utils.IndexedDb;

/// <inheritdoc cref="IIndexedDbUtil"/>
public sealed class IndexedDbUtil : IIndexedDbUtil
{
    private readonly IIndexedDbInterop _interop;
    public IndexedDbUtil(IIndexedDbInterop interop)
    {
        _interop = interop;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Initialize(CancellationToken cancellationToken = default)
    {
        return _interop.Initialize(cancellationToken);
    }

    public ValueTask EnsureStore(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        ValidateName(databaseName, nameof(databaseName));
        ValidateName(storeName, nameof(storeName));

        return _interop.EnsureStore(databaseName, storeName, cancellationToken);
    }

    public ValueTask<string?> Get(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        ValidateDatabaseStoreAndKey(databaseName, storeName, key);
        return _interop.Get(databaseName, storeName, key, cancellationToken);
    }

    public async ValueTask<T?> Get<T>(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        ValidateDatabaseStoreAndKey(databaseName, storeName, key);

        string? value = await _interop.Get(databaseName, storeName, key, cancellationToken);

        if (value is null)
            return default;

        if (typeof(T) == typeof(string))
            return (T?)(object)value;

        if (value.IsNullOrWhiteSpace())
            return default;

        return JsonUtil.Deserialize<T>(value);
    }

    public async ValueTask<IReadOnlyList<T>> GetAll<T>(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        ValidateName(databaseName, nameof(databaseName));
        ValidateName(storeName, nameof(storeName));

        IReadOnlyList<string> values = await _interop.GetAll(databaseName, storeName, cancellationToken);

        if (values.Count == 0)
            return [];

        if (typeof(T) == typeof(string))
            return values.Select(static value => (T)(object)value)
                         .ToList();

        var result = new List<T>(values.Count);

        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            T? item = JsonUtil.Deserialize<T>(value);

            if (item is not null)
                result.Add(item);
        }

        return result;
    }

    public ValueTask Set(string databaseName, string storeName, string key, string value, CancellationToken cancellationToken = default)
    {
        ValidateDatabaseStoreAndKey(databaseName, storeName, key);
        ArgumentNullException.ThrowIfNull(value);

        return _interop.Set(databaseName, storeName, key, value, cancellationToken);
    }

    public ValueTask Set<T>(string databaseName, string storeName, string key, T value, CancellationToken cancellationToken = default)
    {
        ValidateDatabaseStoreAndKey(databaseName, storeName, key);

        if (value is string stringValue)
            return _interop.Set(databaseName, storeName, key, stringValue, cancellationToken);

        string? json = JsonUtil.Serialize(value);
        return _interop.Set(databaseName, storeName, key, json, cancellationToken);
    }

    public ValueTask Remove(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        ValidateDatabaseStoreAndKey(databaseName, storeName, key);
        return _interop.Remove(databaseName, storeName, key, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Clear(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        ValidateName(databaseName, nameof(databaseName));
        ValidateName(storeName, nameof(storeName));

        return _interop.Clear(databaseName, storeName, cancellationToken);
    }

    public ValueTask<bool> ContainsKey(string databaseName, string storeName, string key, CancellationToken cancellationToken = default)
    {
        ValidateDatabaseStoreAndKey(databaseName, storeName, key);
        return _interop.ContainsKey(databaseName, storeName, key, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IReadOnlyList<string>> GetKeys(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        ValidateName(databaseName, nameof(databaseName));
        ValidateName(storeName, nameof(storeName));

        return _interop.GetKeys(databaseName, storeName, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<int> GetLength(string databaseName, string storeName, CancellationToken cancellationToken = default)
    {
        ValidateName(databaseName, nameof(databaseName));
        ValidateName(storeName, nameof(storeName));

        return _interop.GetLength(databaseName, storeName, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask DeleteDatabase(string databaseName, CancellationToken cancellationToken = default)
    {
        ValidateName(databaseName, nameof(databaseName));
        return _interop.DeleteDatabase(databaseName, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateDatabaseStoreAndKey(string databaseName, string storeName, string key)
    {
        ValidateName(databaseName, nameof(databaseName));
        ValidateName(storeName, nameof(storeName));
        ValidateName(key, nameof(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
    }
}