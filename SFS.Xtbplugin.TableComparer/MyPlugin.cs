using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace SFS.Xtbplugin.TableComparer
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    // To generate Base64 string for Images below, you can use https://www.base64-image.de/
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "SFS TableComparer"),
        ExportMetadata("Description", "An XrmToolBox plugin that allows you to compare the fields of two Dataverse tables side by side."),
        // Base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "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"),
        // Base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAFAAAABQCAYAAACOEfKtAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAGHaVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49J++7vycgaWQ9J1c1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCc/Pg0KPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyI+PHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj48cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0idXVpZDpmYWY1YmRkNS1iYTNkLTExZGEtYWQzMS1kMzNkNzUxODJmMWIiIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIj48dGlmZjpPcmllbnRhdGlvbj4xPC90aWZmOk9yaWVudGF0aW9uPjwvcmRmOkRlc2NyaXB0aW9uPjwvcmRmOlJERj48L3g6eG1wbWV0YT4NCjw/eHBhY2tldCBlbmQ9J3cnPz4slJgLAAASBElEQVR4Xu2be3xV1ZXHf2vvs8+5jzzJC7A0QUFpeCQkhADig45aHy2WVtBWKxBeinastZ2ZtvMRaafttFXbGR+VN2jptNhPRWut2s8UqIoECG9RBC2K8sw793Hueew9f9wk3ByTcG9IaD+fud+/kr3W2ufc31ln77X3OQdIkyZNmjRp0qRJkyZNmjRp/n9B3oYLRVXtmsFHdVwi/TRcxdx84bi5AEgxWKbOG4j4SZ/FDo8D/vbn8jvD3vh/FC6ogJ/a/0xVm+Z+3mT2Z5XEGClEjjIEFCOo9jMhACACsxxQ2FQa4RhxttMH8dKl9fLlbVPmfOzp9u/KgAuoACo6uOq2kGB326Ar3KAPyrHBTRuOdAGlAOWNAkAKYAQIAfIZ4IpBaws3a8SeK4rRE++Nm1PnDfl7MKACDn5r7Q1tOnvI8vGJDhQoZEJJFyACKK6R9Aa1w1Q8KxUUSCkACsrQQUE/RMh0deDpYafDS9++/N4PvLEXkgERcMrGn2TuLy142DSMhS4BMhyJZ1o/QYyBcoMINsfqM035reNlNeu8PheKfhewePMTn6n/VMavzUyjXDaHAMeFon48jIonMBQAnw4eNJB1Jryscez8u7yuFwLmbTgfivetnHzy0xmbIn5RLutbAOkAxED9l3xnLzkBZFpQxADJikq3rbrH43lB6DcBL9q1qux0ULxkCVaE5rZ41imCguqcYfuH+HjIJID8TOiNbS9m6771jWZkxuiRIx+qRKXwRgwk/SLg5D1PFzZm4DlL8By0RjpvWdX/IwRIEQCCzM9CoDGyZ91vjn05prPp+o5DLxhZGUtUtfaYN2Yg6RcB9wtzdSwnMFyGwgCof29ZD4oAys9CRrO5M+ujyDX3zyie6miw8x9+4T0WDIAbYlHZpKrbvXEDxXkLWLR7xVwzL/Mm1IfODlD9kHiJFyH+twKEBp6XhUBj9A9lZyLXHb/26w0xzr7TNOyi/0ambyQRg1QuOOP/VVVVNTihuwHjvAQc+8H63FYf/ciNWJBKxUcn6r4uTpXOlUnHrJuTAYMxJ/tM5KHQqLnTX79icdOQvatvVnBjNrtir8zLukxBQboSQmh5Dqel3j4HgvMS8GRb5C4rL2MwzFj8R/YjpAAwAssKgGcHEQxZrw0OO1c2jqnpFMbh6h5o7GEA4EwroPZVje04IEZzxk+aNKJLpwNAnwWcvWmNL6JogQzHQD2sxpImMZgREDCAvEwITYMvYr9e0GzfGr507pUfjJv/ZofbRW+tnMmUG274zNzNAOBwCkDFRw+lFLgudCJ3QULPA0KfBXw1x73SzDKGK9OCTCX72pdwBIArgEGBOEEzDLCcTAihw2+6b2c3RB4tsNwp4cvmXnF89JwNXfpYsoTZii3WLfnDjiZOIEVnSybpSihFt4wYMcJIiOx3+ixghMsbXYOnvkRTAFPxMc4NGkBuFoQmIGz37UBT9OdFYefq6Kia0ubR8x44Xno24xIZdMtFd1jSPTypcdDhm15ZOebbGzdmwlGxxIJTShck+MWBnJzSLsH9TJ8ELH5nzShT165F1Paazo3QIAdlQPf5EAg77+Y0mo8WmeKKl04MHxcaXfPNY+Nq/vrVP6+6+KYX1+d6QwFg4bJlQunGP2eR3ngoy7rCZBQJWFZMEbV0mf3bLxRp9N3yCROuKSsry0mw9htJ33xX7V6T845Bd0QgbzO5mugwEsqy26dIL/ESmhTF/yIFFTSgCR1aS6TJx7SN2VG5YfqW1k2P3XdfLDFy5oYNPDa0tajVZWNbddW8a/K82kR74d6190WD4sG8mHvj0dF3dtoqJlc9yoV+v+M4ie7gPH6XSCU/hsTLJLG2rrb29S5O50F3v74LauZMnvf9G+4N6+x+N+ArdhwbFImBHNnj2EeqfawDgAw/GGMQYXN/jtJWD//Q3rD1uvnHvTHdUbl15Uw4/HjdlXPfAIDSAysGnQTb6nfdOR+XLdyW6Dt+0sR/0XTxE6+AHRBj4JxDui6UK5+3pf2D/bW7zntPsddbePibT4zL/uHnt7TmBn4RIxQ7jW1gLdFexUPHatVvgGVnIBBzdw5qi93+i81O5YlRs3+RrHgAkBE79pzNnUmlW1cMAoBTpH1PSPmsVzwAIKijvQ3HSko4tg2pFJih3awLfWtlddX3vH6p0qMMeftWfjEUFGstv8hWzaF4edBh7PLPWZgCJCfwrAwYIfPDgMuWPvmgf92sZ2e5Xt9kGbFn1ed1hyItwnk3xvXfcYf906lunpFUTamaIInvkECvExu1T2DECBrXYMfsjbHWtq8dPHgw5PVNhm4FLDyw5vbWgPaMySQhHAWo10SNowAyBDSfQLAttnLMmdB3X7/yvjNetwee/lkwVlRUeFqzs05ZVsuWG+4+6vVJpKx27WgpnaoPc2lSwKadJ8bWrPT6AEDlVZX50uLvEmO5qhcBvQhdwIlZW3yNLV/YeuhQm9d+Lj4h4JAdT13XkBf4kw3JEI0BRN0lW1eUAvn9EJCxrLB7T/3Yeau8Lh0opeihzZt5Hd7NabbsS9oMNVRT+mt10xbVe30B4MYXHx+8+9PBFyyhTjWMqpmOXk6nYkp1LeN8ouumlvBCF7BN65Xd24pvAp5NKbhLan3mjceLm7OMX9ukGKIxUG9n264+qfjOsCAVGdqC6b2JBwBEpJZOm+a8OG1R/eufu7fW8Bk7WIDfMmHL6mFeXwB475Ls7EhQqwpG7R+f43QgCbvA6JNZ0QsEwLZsCJ/+ufGTjv6n134uuhwre9/yP7UUZl6PhrZucvOTEABwBiEEMlqjX24sv+v3Xp9kGPfGk4WK67N1zX2ibsKiSKIt961VD5FLlzaOq/lq5c5lIsJEiT+kBllMuX6bO3lRasv4uKHpd/O/2TT28snzDI2vULbT6yTXHUQEIgJsOa2utnaz194TnRlYuG/5jHBe8HrWFDrH3JyAUmDZQQTazEf7Kh4A7Lt88Wkl3e1RS81MbC/evSaHpLpikOQ/G7Vj3ewmMr7pwJkAYiSVOC2EcVqRGSGdZyiAWMzep1wJmez5J6CUAjEGSernADSvvScIAJYoxX56YMVOMzswntoiyV89v4DPlB9VNKjSrVPnpzwAeyndvuxB3dR+uefKeWcAIG//qh9JTZsxyKQN4NidczT6at30rhmaSHV1dZHN8A5xlqNc2adHCZrQYEfNmXu21/3Oa+sOAoBhu1ZcezLP/6odifZaAnjRcjKRfTr0rYbyBY8ktt+z7rE8FfRnHMiPBWyQT1fUtGVa77MtAIysW3mbUnCPTJj/7IgdT11yMsd3hFl4tMRmD+7rpnTxsgRgf5hcvY80Nlq6PT1x7h3GGaTtvsMVLZW2vWXXrl0nvD6JMABoM/hXbJ2lJB6EBt4Ubh0Sxa+8pvz365vOyKyP6vML3lNO9LRNNLy8duXMcW88Wej1TSQCddDmdCkANAb0xzVLPtI6es4DyYgHAEsBSQp/o2TKrh6QrgRpfBQM7X/g1/dXTqleNWHChDFevw5oyaZN2k8Kjxw2/aIEpuW190x2AMEzkRfC5Qtv9pq6Y/T21cN8jvyiZtkv115992GvHQDGvLmiyBXanU1+92gMNL9p9LzPeX3ORcXk6l9yod3V05IuaSg+sWhcg+u4UUj5o7o3a//D68bW+A8Pk0TFsFM7IDEGxrHJ294Tb02sOSayMtdHM/y3XPrO+nyvHQBYa3Mo6hPjTM6+Y5jmN7z2ZFBSnk6mgjgXpAAlFWzbhiLl57r4QeWkib8pLS3VE/2YGeCXyEwfIZUxgwjcdBB06R2vqTe2jbm1McLc57VIZLbXBgC5epa/wafugKM2nqy8522vPRkYob73ajE5ErvoEFIz9FsDWZnrE0xgrisHSy3FMYMRyLRVdsjtdvXQG0cq5h2UkrkjdzxV7bW9VYRrEIseufbgh58saDds4JU7l2XPeOXJwrm/XVVw+0vPZM08sKFLNiBezyU1XvYFy7bBDXHL+Oqqf+9oY7qUOT2+ItUbUkJxnsKgeRaL0R+V8F3vbSep3e13afGzs5ZaAHDx9pVjSmqXzxr55vIFI4c2fK0thmlhJiqU7o4K6+6oU8etyxat+XnJwmVLAp19EOuyv9ifKAJs1wU0/mD5xImlAMAgKbW1D9p3OwxBLrl5XlMyvF9Rc1iR5hu795lPdbQV7F25WCrsG+JqO4r3rls87MC6n0mdXe+AWiXXXs7ysfXvXr5o46vXLnh57YyFr228Zs72v153x35lBT4echydF1JKyTsP1M/Ex0UJTdMEY/RvAEAFu1bMbhgcXCubU9vNYdkZyGyKLWwZM3eF15YMJbtXL2ISZ96vrPl9ce2awfWD+BbD1Tb7bUvTlHzDlNqLp8rvPO2NOxcV1dULuKEtP+9ZuBeIMSjXDTuh6DAGpp0imfqoKwlwXfcab3uykNLeD3N2GQA0ZTrLyCeCwrVr/cdaFn8wrmZ1X8QDAAU5KOU7KkWUlOBCBLWgr5oFrejfeDgWfx6bCmETlqAbRu14ZojXlAxndPvjmJ+hYM/qLzHJi0oOnao8NXr26iM3dn1GkjJEfTqflCFAKVSxL9SXHCXLPQmR9Po5ju3AyQ1kntainTNSKoQy9CaL0QRF8l+LbLrtwDXfOOX16QsEKu6PMiYZiOhi9tiNN8YMRq/D/4mK4JzI1jDacozFhTuXz/DazsXgRsuRkF8CUysOjZ97znVyMswEOEhdJmVfyorUUPHXSLIZAARi9FutfeuCOl/qTgLHha0kGnLFrzIPrJzuNfcG4+6sDNN+uX7sgm636PvCwYqKEgVcnMqWfl8hEBghzABgqpnxR9EaeY+CPqD9oUtSEAFRCy5RIOrTns88tPbH5btWFXjdvOS/vbqmOTfwiDDlU17b+SB0Xs2FZlwQAQmQrvqwU6rCA6sWNuYFl7lNrUD3D916RgGkcVB2EHpL5LjuuuuNGJ4vbdbe2jJtbrPCElb2xiX5H+U6Uy2mFsUy/dcpy0Fhi1VxvHz+bm93fWV8ddWvNUP/ykCWMB207xvO6hRw06ZN2vS892rb8vwV1ByKS5xSNiqQJCifAPkNsHAM3HZOMkedAQNJxoa6QWOQZACiFoSj1FCTxh+tmLfX21VfqBo1Ks/NzToMznPVQI+BRICUrhOKlnQugqdNm+ZkKLbAMF0bhgEVn6mTL6naP4xBzIZqDsF1HViGNtjMNsaaGcYYS2eD3EgUqi0KuBIwBFm26nZXpi9YOcGvcF0MvHgANM4BqTbt37//oy67CCfKanYFQs5i4TPAOUs++7pDKiBmA+EYEIkBlnN2XJAKTkAgZlCVJ6pPFBcX+xj4/Rdi9iUiKKXgKvoxvI81AaCprGalvyX6fZadCSW0ztGwP18cVwRI20GEObd6bX0ht6jgfk0XF/d1Gz8VNKHBtqyn923f/hd0JyAAtIyuWZLREP22EDoQ8IMp2S7k+aSkh7AJKzdYXlC37GteUyqUVVWVa5r2oJRuahNfqihAaAJOzNpnNLCvdzR3KyAANI2e83B+kznD5+AYCnJAQgPrw5q5RxTgOg7aMoxHxm1ZPtxrToaKioohQuMbiDOflAPxVcpZhCHgOPZ+stwvbD+yPV6q9CYgAJwYP39j4bHWif6myFMa4zbyMwFD9F8mRmKw/FrBkYv0F0s3PZ7SC+Hjx48fwXziFRJ8pJPiqxypwDmHpmmwY9ZzvFFOq6ur+zDRnrQSF+1/uqxZdxe5hJsdQxsqdQ3SdQHHjU8YlGrx2H50qYDsIHwNodYMy33A19a6/qMpD0S9rh1UVlYGbMHuFMR+KnQ903KdFH7FuSHEt0eJMSjpQkq5x3Xlw3u37eiyld9Byoee+tr63AP5kalwaarJ2FhFNIxJ6Xfj32AlR/wTurNflCgFxbmhc879kv9v0ZnIcxWnrc3PfOnrDR0hV111VU5DKHQ11/hMxtlniZSrFGId7z6p9vmjveOOqa8j/Bx0XHkipmATqZMKtIsp96Wd23b+BT1/1pz0EXpELVnC5lxdoqMftgPCgQDtGRnKyGkj3/BTkaZnZ93buctbWVkZcISTq9laTAgRNk4YCiUJwUeP4ihKgIQTEUIkdU/Ytk0AUIIShPJDbl1dXR9e/k6TJk2aNGnSpEmTJk2aNGnSpEmT5h+e/wMD7YWsXoeDQAAAAABJRU5ErkJggg=="),
        ExportMetadata("BackgroundColor", "Lavender"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class MyPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}