using System;
using Microsoft.Extensions.Logging;

namespace FortRise.Bugfixes;

internal sealed class BugfixesModule : Mod
{
    public BugfixesModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
    }
}
