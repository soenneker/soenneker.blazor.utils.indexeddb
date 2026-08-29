let indexedDbInterop;
const upgradeQueues = new Map();

function validateName(value, parameterName) {
    if (typeof value !== "string" || value.trim().length === 0) {
        throw new Error(`IndexedDbInterop '${parameterName}' cannot be null, empty, or whitespace.`);
    }
}

function openDatabase(databaseName, version = undefined, onUpgradeNeeded = undefined) {
    return new Promise((resolve, reject) => {
        const request = version == null
            ? indexedDB.open(databaseName)
            : indexedDB.open(databaseName, version);

        request.onupgradeneeded = event => {
            if (typeof onUpgradeNeeded === "function") {
                onUpgradeNeeded(event.target.result);
            }
        };

        request.onsuccess = () => {
            const database = request.result;
            database.onversionchange = () => database.close();
            resolve(database);
        };

        request.onerror = () => reject(request.error ?? new Error(`Failed opening IndexedDB database '${databaseName}'.`));
    });
}

function requestToPromise(request) {
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error ?? new Error("IndexedDB request failed."));
    });
}

function transactionToPromise(transaction) {
    return new Promise((resolve, reject) => {
        transaction.oncomplete = () => resolve();
        transaction.onerror = () => reject(transaction.error ?? new Error("IndexedDB transaction failed."));
        transaction.onabort = () => reject(transaction.error ?? new Error("IndexedDB transaction was aborted."));
    });
}

function deleteIndexedDb(databaseName) {
    return new Promise((resolve, reject) => {
        const request = indexedDB.deleteDatabase(databaseName);

        request.onsuccess = () => resolve();
        request.onerror = () => reject(request.error ?? new Error(`Failed deleting IndexedDB database '${databaseName}'.`));
    });
}

async function enqueueUpgrade(databaseName, operation) {
    const previous = upgradeQueues.get(databaseName) ?? Promise.resolve();
    const current = previous.catch(() => undefined).then(operation);
    upgradeQueues.set(databaseName, current);

    try {
        return await current;
    } finally {
        if (upgradeQueues.get(databaseName) === current) {
            upgradeQueues.delete(databaseName);
        }
    }
}

async function ensureStoreCore(databaseName, storeName) {
    const database = await openDatabase(databaseName, undefined, db => {
        if (!db.objectStoreNames.contains(storeName)) {
            db.createObjectStore(storeName);
        }
    });

    if (database.objectStoreNames.contains(storeName)) {
        database.close();
        return;
    }

    const nextVersion = database.version + 1;
    database.close();

    const upgraded = await openDatabase(databaseName, nextVersion, db => {
        if (!db.objectStoreNames.contains(storeName)) {
            db.createObjectStore(storeName);
        }
    });

    upgraded.close();
}

async function openStoreDatabase(databaseName, storeName, createIfMissing) {
    const database = await openDatabase(databaseName);

    if (database.objectStoreNames.contains(storeName)) {
        return database;
    }

    database.close();

    if (createIfMissing) {
        await indexedDbInterop.ensureStore(databaseName, storeName);
        return openDatabase(databaseName);
    }

    return null;
}

indexedDbInterop = {
    initialize() {
    },

    async ensureStore(databaseName, storeName) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");

        await enqueueUpgrade(databaseName, () => ensureStoreCore(databaseName, storeName));
    },

    async get(databaseName, storeName, key) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");
        validateName(key, "key");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return null;
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            const result = await requestToPromise(store.get(key));

            if (result == null) {
                return null;
            }

            return typeof result === "string" ? result : JSON.stringify(result);
        } finally {
            database.close();
        }
    },

    async getAll(databaseName, storeName) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return [];
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            const values = await requestToPromise(store.getAll());

            return Array.isArray(values)
                ? values.map(value => typeof value === "string" ? value : JSON.stringify(value))
                : [];
        } finally {
            database.close();
        }
    },

    async set(databaseName, storeName, key, value) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");
        validateName(key, "key");

        const database = await openStoreDatabase(databaseName, storeName, true);

        try {
            const transaction = database.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            await requestToPromise(store.put(value ?? "", key));
            await transactionToPromise(transaction);
        } finally {
            database.close();
        }
    },

    async remove(databaseName, storeName, key) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");
        validateName(key, "key");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return;
        }

        try {
            const transaction = database.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            await requestToPromise(store.delete(key));
            await transactionToPromise(transaction);
        } finally {
            database.close();
        }
    },

    async clear(databaseName, storeName) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return;
        }

        try {
            const transaction = database.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            await requestToPromise(store.clear());
            await transactionToPromise(transaction);
        } finally {
            database.close();
        }
    },

    async containsKey(databaseName, storeName, key) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");
        validateName(key, "key");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return false;
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            return await requestToPromise(store.count(key)) > 0;
        } finally {
            database.close();
        }
    },

    async getKeys(databaseName, storeName) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return [];
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            const keys = await requestToPromise(store.getAllKeys());

            return Array.isArray(keys) ? keys.map(key => String(key)) : [];
        } finally {
            database.close();
        }
    },

    async getLength(databaseName, storeName) {
        validateName(databaseName, "databaseName");
        validateName(storeName, "storeName");

        const database = await openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return 0;
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            return await requestToPromise(store.count());
        } finally {
            database.close();
        }
    },

    async deleteDatabase(databaseName) {
        validateName(databaseName, "databaseName");

        await enqueueUpgrade(databaseName, () => deleteIndexedDb(databaseName));
    }
};

export function initialize() {
    return indexedDbInterop.initialize();
}

export function ensureStore(databaseName, storeName) {
    return indexedDbInterop.ensureStore(databaseName, storeName);
}

export function get(databaseName, storeName, key) {
    return indexedDbInterop.get(databaseName, storeName, key);
}

export function getAll(databaseName, storeName) {
    return indexedDbInterop.getAll(databaseName, storeName);
}

export function set(databaseName, storeName, key, value) {
    return indexedDbInterop.set(databaseName, storeName, key, value);
}

export function remove(databaseName, storeName, key) {
    return indexedDbInterop.remove(databaseName, storeName, key);
}

export function clear(databaseName, storeName) {
    return indexedDbInterop.clear(databaseName, storeName);
}

export function containsKey(databaseName, storeName, key) {
    return indexedDbInterop.containsKey(databaseName, storeName, key);
}

export function getKeys(databaseName, storeName) {
    return indexedDbInterop.getKeys(databaseName, storeName);
}

export function getLength(databaseName, storeName) {
    return indexedDbInterop.getLength(databaseName, storeName);
}

export function deleteDatabase(databaseName) {
    return indexedDbInterop.deleteDatabase(databaseName);
}
