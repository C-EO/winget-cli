// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#pragma once
#include <filesystem>
#include <memory>
#include <string>
#include <vector>
#include <stdexcept>
#include <string_view>

namespace AppInstaller::Utility {

    // Forward declaration of type defined within PAL
    struct SHA256Context;

    // Class used to compute SHA256 hashes over various sets of data.
    // Create one and Add data to it if the data is not all available,
    // or simply call ComputeHash if the data is all in memory.
    class SHA256
    {
    public:
        using HashBuffer = std::vector<uint8_t>;
        constexpr static size_t HashBufferSizeInBytes = 32;
        constexpr static size_t HashStringSizeInChars = 64;

        struct HashDetails
        {
            HashBuffer Hash;
            uint64_t SizeInBytes = 0;
        };

        SHA256();

        // Adds the next chunk of data to the hash.
        void Add(const uint8_t* buffer, size_t cbBuffer);

        inline void Add(const std::vector<std::uint8_t>& buffer)
        {
            Add(buffer.data(), buffer.size());
        }

        // Gets the hash of the data. This is a destructive action; the accumulated hash
        // value will be returned and the object can no longer be used.
        void Get(HashBuffer& hash);

        inline HashBuffer Get()
        {
            HashBuffer result{};
            Get(result);
            return result;
        }

        // Computes the hash of the given buffer immediately.
        static HashBuffer ComputeHash(const uint8_t* buffer, std::uint32_t cbBuffer);

        // Computes the hash of the given buffer immediately.
        static HashBuffer ComputeHash(const std::vector<uint8_t>& buffer);

        // Computes the hash of the given string immediately.
        static HashBuffer ComputeHash(std::string_view buffer);

        // Computes the hash from a given stream.
        static HashBuffer ComputeHash(std::istream& in);

        // Computes the hash from a given stream.
        static HashDetails ComputeHashDetails(std::istream& in);

        // Computes the hash from a given file path.
        static HashBuffer ComputeHashFromFile(const std::filesystem::path& path);

        static std::string ConvertToString(const HashBuffer& hashBuffer);

        static std::wstring ConvertToWideString(const HashBuffer& hashBuffer);

        static HashBuffer ConvertToBytes(const std::string& hashStr);

        // Returns a value indicating whether the two hashes are equal.
        static bool AreEqual(const HashBuffer& first, const HashBuffer& second);

    private:
        void EnsureNotFinished() const;

        struct SHA256ContextDeleter
        {
            void operator()(SHA256Context* context);
        };

        std::unique_ptr<SHA256Context, SHA256ContextDeleter> context;
    };
}
