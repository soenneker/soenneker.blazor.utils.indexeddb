export class IndexedDbInterop {
    initialize() {
    }

    async ensureStore(databaseName, storeName) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");

        const version = await this.#getDatabaseVersion(databaseName);

        if (version == null) {
            const database = await this.#openDatabase(databaseName, 1, db => {
                if (!db.objectStoreNames.contains(storeName)) {
                    db.createObjectStore(storeName);
                }
            });

            database.close();
            return;
        }

        const database = await this.#openDatabase(databaseName);

        if (database.objectStoreNames.contains(storeName)) {
            database.close();
            return;
        }

        database.close();

        const upgraded = await this.#openDatabase(databaseName, version + 1, db => {
            if (!db.objectStoreNames.contains(storeName)) {
                db.createObjectStore(storeName);
            }
        });

        upgraded.close();
    }

    async get(databaseName, storeName, key) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");
        this.#validateName(key, "key");

        const database = await this.#openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return null;
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            const result = await this.#requestToPromise(store.get(key));

            if (result == null) {
                return null;
            }

            return typeof result === "string" ? result : JSON.stringify(result);
        } finally {
            database.close();
        }
    }

    async getAll(databaseName, storeName) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");

        const database = await this.#openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return [];
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            const values = await this.#requestToPromise(store.getAll());

            return Array.isArray(values)
                ? values.map(value => typeof value === "string" ? value : JSON.stringify(value))
                : [];
        } finally {
            database.close();
        }
    }

    async set(databaseName, storeName, key, value) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");
        this.#validateName(key, "key");

        await this.ensureStore(databaseName, storeName);

        const database = await this.#openStoreDatabase(databaseName, storeName, true);

        try {
            const transaction = database.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            await this.#requestToPromise(store.put(value ?? "", key));
            await this.#transactionToPromise(transaction);
        } finally {
            database.close();
        }
    }

    async remove(databaseName, storeName, key) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");
        this.#validateName(key, "key");

        const database = await this.#openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return;
        }

        try {
            const transaction = database.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            await this.#requestToPromise(store.delete(key));
            await this.#transactionToPromise(transaction);
        } finally {
            database.close();
        }
    }

    async clear(databaseName, storeName) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");

        const database = await this.#openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return;
        }

        try {
            const transaction = database.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            await this.#requestToPromise(store.clear());
            await this.#transactionToPromise(transaction);
        } finally {
            database.close();
        }
    }

    async containsKey(databaseName, storeName, key) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");
        this.#validateName(key, "key");

        const value = await this.get(databaseName, storeName, key);
        return value != null;
    }

    async getKeys(databaseName, storeName) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");

        const database = await this.#openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return [];
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            const keys = await this.#requestToPromise(store.getAllKeys());

            return Array.isArray(keys) ? keys.map(key => String(key)) : [];
        } finally {
            database.close();
        }
    }

    async getLength(databaseName, storeName) {
        this.#validateName(databaseName, "databaseName");
        this.#validateName(storeName, "storeName");

        const database = await this.#openStoreDatabase(databaseName, storeName, false);

        if (database == null) {
            return 0;
        }

        try {
            const transaction = database.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            return await this.#requestToPromise(store.count());
        } finally {
            database.close();
        }
    }

    async deleteDatabase(databaseName) {
        this.#validateName(databaseName, "databaseName");

        const version = await this.#getDatabaseVersion(databaseName);

        if (version == null) {
            return;
        }

        await this.#deleteDatabase(databaseName);
    }

    async #openStoreDatabase(databaseName, storeName, createIfMissing) {
        const version = await this.#getDatabaseVersion(databaseName);

        if (version == null) {
            if (createIfMissing) {
                await this.ensureStore(databaseName, storeName);
                return this.#openDatabase(databaseName);
            }

            return null;
        }

        const database = await this.#openDatabase(databaseName);

        if (database.objectStoreNames.contains(storeName)) {
            return database;
        }

        database.close();

        if (createIfMissing) {
            await this.ensureStore(databaseName, storeName);
            return this.#openDatabase(databaseName);
        }

        return null;
    }

    async #getDatabaseVersion(databaseName) {
        if (typeof indexedDB.databases !== "function") {
            return null;
        }

        const databases = await indexedDB.databases();
        const match = databases.find(database => database.name === databaseName);

        return match?.version ?? null;
    }

    #openDatabase(databaseName, version = undefined, onUpgradeNeeded = undefined) {
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
            request.onblocked = () => reject(new Error(`Opening IndexedDB database '${databaseName}' was blocked.`));
        });
    }

    #requestToPromise(request) {
        return new Promise((resolve, reject) => {
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error ?? new Error("IndexedDB request failed."));
        });
    }

    #transactionToPromise(transaction) {
        return new Promise((resolve, reject) => {
            transaction.oncomplete = () => resolve();
            transaction.onerror = () => reject(transaction.error ?? new Error("IndexedDB transaction failed."));
            transaction.onabort = () => reject(transaction.error ?? new Error("IndexedDB transaction was aborted."));
        });
    }

    #deleteDatabase(databaseName) {
        return new Promise((resolve, reject) => {
            const request = indexedDB.deleteDatabase(databaseName);

            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error ?? new Error(`Failed deleting IndexedDB database '${databaseName}'.`));
            request.onblocked = () => reject(new Error(`Deleting IndexedDB database '${databaseName}' was blocked.`));
        });
    }

    #validateName(value, parameterName) {
        if (typeof value !== "string" || value.trim().length === 0) {
            throw new Error(`IndexedDbInterop '${parameterName}' cannot be null, empty, or whitespace.`);
        }
    }
}

window.IndexedDbInterop = new IndexedDbInterop();
