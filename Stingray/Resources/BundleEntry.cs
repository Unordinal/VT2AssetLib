using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VT2AssetLib.Stingray.Resources;

public sealed class BundleEntry
{
    public ResourceLocator Locator { get; set; }

    public uint VariantCount { get; set; }

    public uint StreamOffset { get; set; }

    public BundleEntryVariant[] Variants { get; set; } = null!;
}
