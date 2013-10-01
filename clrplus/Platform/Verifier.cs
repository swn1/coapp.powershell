﻿//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Changes Copyright (c) 2011 Garrett Serack. All rights reserved.
// </copyright>
// Original Source: http://www.pinvoke.net/default.aspx/wintrust.winverifytrust
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------

namespace ClrPlus.Crypto {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.Pkcs;
    using System.Security.Cryptography.X509Certificates;
    using Core.Collections;
    using Windows.Api;
    using Windows.Api.Enumerations;
    using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

    public class Verifier {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        // GUID of the action to perform
        private const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

        private static HashSet<string> _isValidCache = new HashSet<string>();

        public static bool HasValidSignature(string fileName) {
            try {
                if (_isValidCache.Contains(fileName)) {
                    return true;
                }
                var wtd = new WinTrustData(fileName);
                var guidAction = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
                WinVerifyTrustResult result = WinTrust.WinVerifyTrust(INVALID_HANDLE_VALUE, guidAction, wtd);
                bool ret = (result == WinVerifyTrustResult.Success);

                if (ret) {
                    _isValidCache.Add(fileName);
                }
#if COAPP_ENGINE_CORE
                var response = Event<GetResponseInterface>.RaiseFirst();

                if( response != null ) {
                    response.SignatureValidation(fileName, ret, ret ? Verifier.GetPublisherInformation(fileName)["PublisherName"] : null);
                }
#endif
                return ret;
            } catch (Exception) {
                return false;
            }
        }

        public static IDictionary<string, string> GetPublisherInformation(string filename) {
            var result = new XDictionary<string, string>();
            try {
                var cert = new X509Certificate2(filename);
                var fields = cert.Subject.Split(new[] {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries);
                // var result = fields.Select(f => f.Split('=')).Where(s => s.Length > 1).ToDictionary(s => s[0], s => s[1]);

                result.Add("PublisherName", fields[0].Split('=')[1]);
            } catch (Exception) {
            }

            return result;
        }

        public static string GetPublisherName(string filename) {
            try {
                var cert = new X509Certificate2(filename);
                var fields = cert.Subject.Split(new[] {
                    ','
                }, StringSplitOptions.RemoveEmptyEntries);
                return fields[0].Split('=')[1];
            } catch (Exception) {
            }

            return null;
        }

        public static void GetSignatureInformation(string filename) {
            if (HasValidSignature(filename)) {
                var cert = new X509Certificate2(filename);
                Console.WriteLine("Cert: {0}", cert.Subject);

                var ch = new X509Chain();

                ch.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;

                ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                ch.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);

                ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;

                ch.Build(cert);

                ch.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                Console.WriteLine("Chain Information");

                Console.WriteLine("Chain revocation flag: {0}", ch.ChainPolicy.RevocationFlag);
                Console.WriteLine("Chain revocation mode: {0}", ch.ChainPolicy.RevocationMode);
                Console.WriteLine("Chain verification flag: {0}", ch.ChainPolicy.VerificationFlags);
                Console.WriteLine("Chain verification time: {0}", ch.ChainPolicy.VerificationTime);
                Console.WriteLine("Chain status length: {0}", ch.ChainStatus.Length);
                Console.WriteLine("Chain application policy count: {0}", ch.ChainPolicy.ApplicationPolicy.Count);
                Console.WriteLine("Chain certificate policy count: {0} {1}", ch.ChainPolicy.CertificatePolicy.Count, Environment.NewLine);
                //Output chain element information.
                Console.WriteLine("Chain Element Information");
                Console.WriteLine("Number of chain elements: {0}", ch.ChainElements.Count);

                foreach (var element in ch.ChainElements) {
                    Console.WriteLine("Element issuer name: {0}", element.Certificate.Issuer);
                    Console.WriteLine("Element certificate valid until: {0}", element.Certificate.NotAfter);
                    Console.WriteLine("Element certificate is valid: {0}", element.Certificate.Verify());
                    Console.WriteLine("Element error status length: {0}", element.ChainElementStatus.Length);
                    Console.WriteLine("Element information: {0}", element.Information);
                    Console.WriteLine("Number of element extensions: {0}{1}", element.Certificate.Extensions.Count, Environment.NewLine);
                }

                if (ch.ChainStatus.Length > 0) {
                    for (int index = 0; index < ch.ChainStatus.Length; index++) {
                        Console.WriteLine(ch.ChainStatus[index].Status);
                        Console.WriteLine(ch.ChainStatus[index].StatusInformation);
                    }
                }

                Console.WriteLine("Cert Valid?: {0}", cert.Verify());
            }
        }

        public static bool IsTimestamped(string filename) {
            try {
                int encodingType;
                int contentType;
                int formatType;
                IntPtr certStore = IntPtr.Zero;
                IntPtr cryptMsg = IntPtr.Zero;
                IntPtr context = IntPtr.Zero;

                if (!WinCrypt.CryptQueryObject(
                    WinCrypt.CERT_QUERY_OBJECT_FILE,
                    Marshal.StringToHGlobalUni(filename),
                    WinCrypt.CERT_QUERY_CONTENT_FLAG_ALL,
                    WinCrypt.CERT_QUERY_FORMAT_FLAG_ALL,
                    0,
                    out encodingType,
                    out contentType,
                    out formatType,
                    ref certStore,
                    ref cryptMsg,
                    ref context)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                //expecting contentType=10; CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED 
                //Logger.LogInfo(string.Format("Querying file '{0}':", filename));
                //Logger.LogInfo(string.Format("  Encoding Type: {0}", encodingType));
                //Logger.LogInfo(string.Format("  Content Type: {0}", contentType));
                //Logger.LogInfo(string.Format("  Format Type: {0}", formatType));
                //Logger.LogInfo(string.Format("  Cert Store: {0}", certStore.ToInt32()));
                //Logger.LogInfo(string.Format("  Crypt Msg: {0}", cryptMsg.ToInt32()));
                //Logger.LogInfo(string.Format("  Context: {0}", context.ToInt32()));

                // Get size of the encoded message.
                int cbData = 0;
                if (!WinCrypt.CryptMsgGetParam(
                    cryptMsg,
                    WinCrypt.CMSG_ENCODED_MESSAGE, //Crypt32.CMSG_SIGNER_INFO_PARAM,
                    0,
                    IntPtr.Zero,
                    ref cbData)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                var vData = new byte[cbData];

                // Get the encoded message.
                if (!WinCrypt.CryptMsgGetParam(
                    cryptMsg,
                    WinCrypt.CMSG_ENCODED_MESSAGE, //Crypt32.CMSG_SIGNER_INFO_PARAM,
                    0,
                    vData,
                    ref cbData)) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                var signedCms = new SignedCms();
                signedCms.Decode(vData);

                foreach (var signerInfo in signedCms.SignerInfos) {
                    foreach (var unsignedAttribute in signerInfo.UnsignedAttributes) {
                        if (unsignedAttribute.Oid.Value == WinCrypt.szOID_RSA_counterSign) {
                            foreach (var counterSignInfo in signerInfo.CounterSignerInfos) {
                                foreach (var signedAttribute in counterSignInfo.SignedAttributes) {
                                    if (signedAttribute.Oid.Value == WinCrypt.szOID_RSA_signingTime) {
                                        var fileTime = new FILETIME();
                                        int fileTimeSize = Marshal.SizeOf(fileTime);
                                        IntPtr fileTimePtr = Marshal.AllocCoTaskMem(fileTimeSize);
                                        Marshal.StructureToPtr(fileTime, fileTimePtr, true);

                                        var buffdata = new byte[fileTimeSize];
                                        Marshal.Copy(fileTimePtr, buffdata, 0, fileTimeSize);

                                        var buffSize = (uint)buffdata.Length;

                                        uint encoding = WinCrypt.X509_ASN_ENCODING | WinCrypt.PKCS_7_ASN_ENCODING;

                                        var rsaSigningTime = (UIntPtr)(uint)Marshal.StringToHGlobalAnsi(WinCrypt.szOID_RSA_signingTime);

                                        byte[] pbData = signedAttribute.Values[0].RawData;
                                        var ucbData = (uint)pbData.Length;

                                        bool workie = WinCrypt.CryptDecodeObject(encoding, rsaSigningTime, pbData, ucbData, 0, buffdata, ref buffSize);

                                        if (workie) {
                                            IntPtr fileTimePtr2 = Marshal.AllocCoTaskMem(buffdata.Length);
                                            Marshal.Copy(buffdata, 0, fileTimePtr2, buffdata.Length);
                                            var fileTime2 = (FILETIME)Marshal.PtrToStructure(fileTimePtr2, typeof (FILETIME));

                                            long hFT2 = (((long)fileTime2.dwHighDateTime) << 32) + ((uint)fileTime2.dwLowDateTime);

                                            DateTime dte = DateTime.FromFileTime(hFT2);
                                            Console.WriteLine(dte.ToString());
                                        } else {
                                            throw new Win32Exception(Marshal.GetLastWin32Error());
                                        }
                                    }
                                }
                            }

                            return true;
                        }
                    }
                }
            } catch (Exception) {
                // no logging
            }

            return false;
        }
    }
}