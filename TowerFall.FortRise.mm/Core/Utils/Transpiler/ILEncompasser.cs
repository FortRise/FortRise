#nullable enable
using System;

namespace FortRise.Transpiler;

internal readonly struct ILEncompasser(ILTranspilerCursor cursor)
{
    private readonly ILTranspilerCursor Cursor = cursor;

    public readonly bool Next(MoveType moveType, params Span<InstructionMatcher> instrMatches)
    {
        Cursor.GotoNext(moveType, out bool res, instrMatches);
        return res;
    }

    public readonly bool Prev(MoveType moveType, params Span<InstructionMatcher> instrMatches)
    {
        Cursor.GotoPrev(moveType, out bool res, instrMatches);
        return res;
    }
}