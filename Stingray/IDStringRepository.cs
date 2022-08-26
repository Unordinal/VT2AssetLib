using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace VT2AssetLib.Stingray;

/// <summary>
/// A repository for adding and getting Murmur string hashes.
/// </summary>
public class IDStringRepository
{
    /// <summary>
    /// Gets the shared <see cref="IDStringRepository"/>.
    /// </summary>
    public static IDStringRepository Shared { get; } = new();

    private readonly Dictionary<ulong, IDString64> _idString64Repo;
    private readonly Dictionary<uint, IDString32> _idString32Repo;
    private readonly object _lock = new();

    public IDStringRepository()
    {
        _idString64Repo = new();
        _idString32Repo = new();
    }

    public IDStringRepository(IEnumerable<IDString64> idStrings)
    {
        _idString64Repo = idStrings.ToDictionary(idString => idString.ID);
        _idString32Repo = new();
    }

    public IDStringRepository(IEnumerable<IDString32> idStrings)
    {
        _idString64Repo = new();
        _idString32Repo = idStrings.ToDictionary(idString => idString.ID);
    }

    public IDStringRepository(IEnumerable<IDString64> idString64s, IEnumerable<IDString32> idString32s)
    {
        _idString64Repo = idString64s.ToDictionary(idString => idString.ID);
        _idString32Repo = idString32s.ToDictionary(idString => idString.ID);
    }

    public void Add(IDString64 idString)
    {
        if (!InternalAdd(idString))
            ThrowHashAlreadyExistsException(idString);
    }

    public void Add(IDString32 idString)
    {
        if (!InternalAdd(idString))
            ThrowHashAlreadyExistsException(idString);
    }

    // Each line should be in the form
    //      0123456789abcdef: my_example_string
    //      or
    //      my_example_string
    public void AddIDString64DictionaryFile(string filePath, bool throwOnDuplicateHashes = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var fileLines = File.ReadLines(filePath);
        int lineNum = 0;
        foreach (var line in fileLines)
        {
            lineNum++;
            if (line.Length == 0 || line.All(char.IsWhiteSpace))
                continue;

            string trimmedLine = line.Trim();
            ulong hash;
            string hashValue;

            // If > 16, try parsing the first 16 characters as a hash.
            if (trimmedLine.Length > 16)
            {
                var hashString = trimmedLine[..16];
                if (ulong.TryParse(hashString, NumberStyles.HexNumber, null, out hash))
                {
                    if (line[16] != ':')
                        ThrowMalformedDictionary64FileException(lineNum);

                    hashValue = trimmedLine[17..].TrimStart();
                    AddHashAndValue(hash, hashValue);
                    continue;
                }
            }

            // If we're here, the line is either <= 16 length or we couldn't parse a valid hash from the starting 16 characters.
            hashValue = trimmedLine;
            hash = Murmur.Hash64(hashValue);
            AddHashAndValue(hash, hashValue);
        }

        void AddHashAndValue(ulong hash, string hashValue)
        {
            IDString64 idString = new(hash, hashValue);
            if (throwOnDuplicateHashes)
                Add(idString);
            else
                TryAdd(idString);
        }
    }
    
    public void AddIDString32DictionaryFile(string filePath, bool throwOnDuplicateHashes = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var fileLines = File.ReadLines(filePath);
        int lineNum = 0;
        foreach (var line in fileLines)
        {
            lineNum++;
            if (line.Length == 0 || line.All(char.IsWhiteSpace))
                continue;

            string trimmedLine = line.Trim();
            uint hash;
            string hashValue;

            // If > 8, try parsing the first 8 characters as a hash.
            if (trimmedLine.Length > 8)
            {
                var hashString = trimmedLine[..8];
                if (uint.TryParse(hashString, NumberStyles.HexNumber, null, out hash))
                {
                    if (line[8] != ':')
                        ThrowMalformedDictionary32FileException(lineNum);

                    hashValue = trimmedLine[9..].TrimStart();
                    AddHashAndValue(hash, hashValue);
                    continue;
                }
            }

            // If we're here, the line is either <= 8 length or we couldn't parse a valid hash from the starting 8 characters.
            hashValue = trimmedLine;
            hash = Murmur.Hash32(hashValue);
            AddHashAndValue(hash, hashValue);
        }

        void AddHashAndValue(uint hash, string hashValue)
        {
            IDString32 idString = new(hash, hashValue);
            if (throwOnDuplicateHashes)
                Add(idString);
            else
                TryAdd(idString);
        }
    }

    public void AddDictionaryFile(string filePath, bool throwOnDuplicateHashes = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("The dictionary path is null or white space.", nameof(filePath));

        var fileLines = File.ReadLines(filePath);
        int lineNum = 0;
        foreach (var line in fileLines)
        {
            lineNum++;
            if (line.Length == 0 || line.All(char.IsWhiteSpace))
                continue;

            string trimmedLine = line.Trim();
            AddLineFromDictionaryFile(trimmedLine, lineNum);
        }
    }

    public bool TryAdd(IDString64 idString)
    {
        return InternalAdd(idString);
    }

    public bool TryAdd(IDString32 idString)
    {
        return InternalAdd(idString);
    }

    public IDString64 Get(ulong hash)
    {
        lock (_lock)
        {
            return _idString64Repo[hash];
        }
    }

    public IDString32 Get(uint hash)
    {
        lock (_lock)
        {
            return _idString32Repo[hash];
        }
    }

    // Default value will use the passed hash.
    public IDString64 GetOrCreate(ulong hash)
    {
        if (TryGet(hash, out var idString))
            return idString;

        return new IDString64(hash);
    }

    public IDString32 GetOrCreate(uint hash)
    {
        if (TryGet(hash, out var idString))
            return idString;

        return new IDString32(hash);
    }

    public bool TryGet(ulong hash, out IDString64 value)
    {
        lock (_lock)
        {
            return _idString64Repo.TryGetValue(hash, out value);
        }
    }

    public bool TryGet(uint hash, out IDString32 value)
    {
        lock (_lock)
        {
            return _idString32Repo.TryGetValue(hash, out value);
        }
    }

    private bool InternalAdd(IDString64 idString)
    {
        bool result = false;
        lock (_lock)
        {
            result = _idString64Repo.TryAdd(idString.ID, idString);
        }

        if (!result)
        {
            var existing = _idString64Repo[idString.ID];
            if (idString.Value != existing.Value)
                Trace.WriteLine($"Hash collision: the specified 64-bit hash already exists with a different value in the repository. Existing: <{existing.ID:x16}: {existing}>. Passed: <{idString.ID:x16}: {idString}>.");
        }

        return result;
    }

    private bool InternalAdd(IDString32 idString)
    {
        bool result = false;
        lock (_lock)
        {
            result = _idString32Repo.TryAdd(idString.ID, idString);
        }

        if (!result)
        {
            var existing = _idString32Repo[idString.ID];
            if (idString.Value != existing.Value)
                Trace.WriteLine($"Hash collision: the specified 32-bit hash already exists with a different value in the repository. Existing: <{existing.ID:x8}: {existing}>. Passed: <{idString.ID:x8}: {idString}>.");
        }

        return result;
    }

    private void AddLineFromDictionaryFile(string line, int lineNum, bool throwOnDuplicateHashes = false)
    {
        int indexOfColon = line.IndexOf(':');

        // 32-bit hash prefix.
        if (indexOfColon == 8 && (line.Length > 9) && uint.TryParse(line[..indexOfColon], NumberStyles.HexNumber, null, out uint hash32))
        {
            string hashValue = line[(indexOfColon + 1)..].TrimStart();
            if (hashValue.Length != 0)
            {
                IDString32 idString = new(hash32, hashValue);
                if (throwOnDuplicateHashes)
                    Add(idString);
                else
                    TryAdd(idString);

                return;
            }
            else
                Trace.WriteLine($"Line at '{lineNum}' is prefixed with a 32-bit hash but has no associated value.");
        }

        // 64-bit hash prefix.
        if (indexOfColon == 16 && line.Length > 17 && ulong.TryParse(line[..indexOfColon], NumberStyles.HexNumber, null, out ulong hash64))
        {
            string hashValue = line[(indexOfColon + 1)..].TrimStart();
            if (hashValue.Length != 0)
            {
                IDString64 idString = new(hash64, hashValue);
                if (throwOnDuplicateHashes)
                    Add(idString);
                else
                    TryAdd(idString);

                return;
            }
            else
                Trace.WriteLine($"Line at '{lineNum}' is prefixed with a 64-bit hash but has no associated value.");
        }

        // No hash prefix, hash the entire line as 32-bit and 64-bit and add to both dictionaries.
        hash64 = Murmur.Hash64(line);
        hash32 = Murmur.GetAsHash32(hash64);

        IDString64 idString64 = new(hash64, line);
        IDString32 idString32 = new(hash32, line);

        if (throwOnDuplicateHashes)
        {
            Add(idString64);
            Add(idString32);
        }
        else
        {
            TryAdd(idString64);
            TryAdd(idString32);
        }
    }

    [DebuggerHidden]
    private void ThrowHashAlreadyExistsException(IDString64 idString, [CallerArgumentExpression("idString")] string? argName = null)
    {
        Debug.Assert(_idString64Repo.ContainsKey(idString.ID));

        var existing = _idString64Repo[idString.ID];
        string existingStr = existing.Value ?? "<null>";
        string passedStr = idString.Value ?? "<null>";
        if (existing.Value == idString.Value)
            throw new ArgumentException($"The specified {nameof(IDString64)} already exists in the repo with the same value of '{existingStr}'.", argName);
        else
            throw new ArgumentException($"The specified {nameof(IDString64)} already exists in the repo with a different value. Existing: <{existingStr}>. Passed: <{passedStr}>.", argName);
    }

    [DebuggerHidden]
    private void ThrowHashAlreadyExistsException(IDString32 idString, [CallerArgumentExpression("idString")] string? argName = null)
    {
        Debug.Assert(_idString32Repo.ContainsKey(idString.ID));

        var existing = _idString32Repo[idString.ID];
        string existingStr = existing.Value ?? "<null>";
        string passedStr = idString.Value ?? "<null>";
        if (existing.Value == idString.Value)
            throw new ArgumentException($"The specified {nameof(IDString32)} already exists in the repo with the same value. Existing: <{existingStr}>.", argName);
        else
            throw new ArgumentException($"The specified {nameof(IDString32)} already exists in the repo with a different value. Existing: <{existingStr}>. Passed: <{passedStr}>.", argName);
    }

    [DebuggerHidden]
    private void ThrowMalformedDictionary64FileException(int lineNumber)
    {
        throw new InvalidDataException($"""
            Malformed dictionary file: no colon at position 16 on line {lineNumber}.
            Dictionary files need to be in one of two formats (excluding quotes):
                - "0123456789abcdef: the_hash_value" Exactly 16 characters with a colon directly after and then the original value of the hash. Leading and trailing white space is ignored.
                - "the_hash_value" The original hash value alone. This value will be hashed. Leading and trailing white space is ignored.
            """
            );
    }
    
    [DebuggerHidden]
    private void ThrowMalformedDictionary32FileException(int lineNumber)
    {
        throw new InvalidDataException($"""
            Malformed dictionary file: no colon at position 8 on line {lineNumber}.
            Dictionary files need to be in one of two formats (excluding quotes):
                - "0123abcd: the_hash_value" Exactly 8 characters with a colon directly after and then the original value of the hash. Leading and trailing white space is ignored.
                - "the_hash_value" The original hash value alone. This value will be hashed. Leading and trailing white space is ignored.
            """
            );
    }
}