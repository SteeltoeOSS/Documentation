---
_disableAffix: true
---

# Windows Network File Shares

The Steeltoe `WindowsNetworkFileShare` class provides a simplified experience for interacting with SMB file shares by making [P/Invoke calls](https://learn.microsoft.com/cpp/dotnet/how-to-call-native-dlls-from-managed-code-using-pinvoke) to underlying Windows APIs, specifically to `mpr.dll`. For more information about SMB, see the [Microsoft SMB Protocol documentation](https://learn.microsoft.com/windows/win32/fileio/microsoft-smb-protocol-and-cifs-protocol-overview).

Network shares are not the most cloud-native way to deal with files. For new development, consider exploring message queues, caches, blob stores, and NoSQL stores. The alternatives offer greater resiliency and decoupling from backing services. That said, sometimes the alternatives are not viable. For .NET applications deployed to Microsoft Windows Servers, Steeltoe provides a stepping stone towards cloud-native deployment in the form of the `WindowsNetworkFileShare`. For applications deployed to Linux hosts on the Tanzu Platform, [volume services](https://techdocs.broadcom.com/us/en/vmware-tanzu/platform/tanzu-platform-for-cloud-foundry/10-0/tpcf/enable-vol-services.html) are available.
