using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System;

public class FileStorageSample : MonoBehaviour
{
    [SerializeField]
    private FileHub fileHub;

    [SerializeField]
    private InputField input;

    [SerializeField]
    private Text showText;

    private User user;
    private Account account;
    private FsFile latestFile;

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        // wait for filehub
        while (fileHub.Blockchain == null) yield return new WaitForSeconds(0.5f);

        KeyPair keyPair = new KeyPair();
        SingleSignatureAuthDescriptor singleSigAuthDescriptor = new SingleSignatureAuthDescriptor(
            keyPair.PubKey,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray(),
            null
        );
        User user = new User(keyPair, singleSigAuthDescriptor);
        Account account = null;
        yield return fileHub.Blockchain.RegisterAccount(
            user.AuthDescriptor, user,
            (Account _account) => account = _account,
            (string error) => throw new Exception(error));

        this.user = user;
        this.account = account;
    }

    public void SaveTextWrapper()
    {
        if (this.user == null) return;
        StartCoroutine(SaveText());
    }

    private IEnumerator SaveText()
    {
        var data = Encoding.ASCII.GetBytes(input.text);
        var file = FsFile.FromData(data);

        yield return fileHub.StoreFile(
            user,
            file,
            () => Debug.Log("Stored file with hash: " + Util.ByteArrayToString(file.Hash)),
            (string error) => Debug.Log(error));

        input.text = "";
        latestFile = file;
    }

    public void LoadLatestTextWrapper()
    {
        if (latestFile == null) return;
        StartCoroutine(LoadLatestText());
    }

    private IEnumerator LoadLatestText()
    {
        FsFile storedFile = null;
        yield return fileHub.GetFile(latestFile.Hash,
            (FsFile _file) => storedFile = _file,
            (string error) => Debug.Log(error));

        showText.text = Encoding.ASCII.GetString(storedFile.Data);
    }
}
