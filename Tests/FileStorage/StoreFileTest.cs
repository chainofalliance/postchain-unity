using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using Chromia.Postchain.Fs;
using NUnit.Framework;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;
using System;

// FileHub has to be initialised
public class StoreFile
{
    private const string FILEHUB_NODE = "http://127.0.0.1:7740";
    private const string FILEHUB_BRID = "ED5C6FF9862E0E545C472E3FB033A776CD7FAB28AFE28124ABF6245A26CA579D";

    private static System.Random random = new System.Random();

    private FileHub fileHub;
    private User user;
    private Account account;

    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private byte[] GenerateData(int length)
    {
        var data = GenerateRandomString(length);
        UnityEngine.Debug.Log(data);
        return Encoding.ASCII.GetBytes(data);
    }

    private IEnumerator SetupFileHub()
    {
        FileHub fileHub = new GameObject().AddComponent<FileHub>();
        yield return fileHub.Establish(FILEHUB_NODE, FILEHUB_BRID);
        this.fileHub = fileHub;
    }

    private IEnumerator SetupAccount()
    {
        User user = TestUser.SingleSig();
        Account account = null;
        yield return fileHub.Blockchain.RegisterAccount(
            user.AuthDescriptor, user,
            (Account _account) => account = _account,
            (string error) => throw new Exception(error));

        this.user = user;
        this.account = account;
    }

    // Create file
    [UnityTest]
    public IEnumerator StoreFileTest1()
    {
        var data = GenerateData(11);
        var file = FsFile.FromData(data);

        Assert.AreEqual(file.NumberOfChunks(), 1);
        Assert.AreEqual(Util.ByteArrayToString(file.Data), Util.ByteArrayToString(data));
        yield return null;
    }

    // Store file
    [UnityTest]
    public IEnumerator StoreFileTest2()
    {
        var data = GenerateData(36);
        var file = FsFile.FromData(data);

        yield return SetupFileHub();
        yield return SetupAccount();

        yield return fileHub.StoreFile(
            user,
            file,
            () => Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash)),
            (string error) => Debug.Log(error));

        FsFile storedFile = null;
        yield return fileHub.GetFile(file.Hash,
            (FsFile _file) => storedFile = _file,
            (string error) => Debug.Log(error));

        Assert.AreEqual(Util.ByteArrayToString(storedFile.Data), Util.ByteArrayToString(file.Data));
    }

    // Store actual file
    [UnityTest]
    public IEnumerator StoreFileTest3()
    {
        var path = Path.Combine(UnityEngine.Application.dataPath, "postchain-unity/Tests/FileStorage/files/small.txt");
        var file = FsFile.FromLocalFile(path);

        yield return SetupFileHub();
        yield return SetupAccount();

        yield return fileHub.StoreFile(
            user,
            file,
            () => Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash)),
            (string error) => Debug.Log(error));

        FsFile storedFile = null;
        yield return fileHub.GetFile(file.Hash,
            (FsFile _file) => storedFile = _file,
            (string error) => Debug.Log(error));

        Assert.AreEqual(Util.ByteArrayToString(storedFile.Data), Util.ByteArrayToString(file.Data));
    }

    // Store actual file, large
    [UnityTest]
    public IEnumerator StoreFileTest4()
    {
        var path = Path.Combine(UnityEngine.Application.dataPath, "postchain-unity/Tests/FileStorage/files/large.txt");
        var file = FsFile.FromLocalFile(path);

        Debug.Log(file.NumberOfChunks());

        yield return SetupFileHub();
        yield return SetupAccount();

        yield return fileHub.StoreFile(
            user,
            file,
            () => Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash)),
            (string error) => Debug.Log(error));

        FsFile storedFile = null;
        yield return fileHub.GetFile(file.Hash,
            (FsFile _file) => storedFile = _file,
            (string error) => Debug.Log(error));

        Assert.AreEqual(Util.ByteArrayToString(storedFile.Data), Util.ByteArrayToString(file.Data));
    }

    // Store file, large file split into multiple chunks
    [UnityTest]
    public IEnumerator StoreFileTest5()
    {
        var dataSize = 1024 * 1024 * 2;
        var data = GenerateData(dataSize);
        var file = FsFile.FromData(data);

        yield return SetupFileHub();
        yield return SetupAccount();

        yield return fileHub.StoreFile(
            user,
            file,
            () => Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash)),
            (string error) => Debug.Log(error));

        FsFile storedFile = null;
        yield return fileHub.GetFile(file.Hash,
            (FsFile _file) => storedFile = _file,
            (string error) => Debug.Log(error));

        Assert.AreEqual(storedFile.NumberOfChunks(), 21);
        Assert.AreEqual(storedFile.NumberOfChunks(), file.NumberOfChunks());
    }
}
