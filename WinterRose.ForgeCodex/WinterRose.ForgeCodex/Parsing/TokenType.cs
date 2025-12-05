namespace WinterRose.ForgeCodex.Parsing
{
    public enum TokenType
    {
        Identifier,
        Number,
        String,
        Boolean,
        Arrow,      // ->
        LBrace,     // {
        RBrace,     // }
        LParen,     // (
        RParen,     // )
        Comma,
        Operator,   // math op
        KeywordFrom,
        KeywordWhere,
        KeywordTake,
        EndOfFile,
        KeywordAdd,
        KeywordTo,
        KeywordUpdate,
        KeywordRemove,
        KeywordCreate,
        KeywordDrop,
        Colon,
        KeywordTable,
        LBracket,   // [
        RBracket,   // ]
        KeywordFor,
        KeywordIf,
        KeywordThen,
        KeywordElse,
        KeywordExcept,
        KeywordOrder,
        KeywordBy,
        KeywordLimit,
        KeywordCount,
        KeywordExists,
        KeywordOr,
        KeywordDescending
    }
}