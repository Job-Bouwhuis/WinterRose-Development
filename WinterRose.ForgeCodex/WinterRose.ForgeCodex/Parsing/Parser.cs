using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WinterRose.ForgeCodex.Parsing.Ast;
using WinterRose.ForgeCodex.Parsing.Modifiers;

namespace WinterRose.ForgeCodex.Parsing;

public sealed class Parser
{
    private readonly Scanner scanner;
    private Token current;

    public Parser(string source)
    {
        scanner = new Scanner(source);
        current = scanner.ReadToken();
    }

    private Token Advance()
    {
        Token prev = current;
        current = scanner.ReadToken();
        return prev;
    }

    private bool Match(TokenType type)
    {
        if (current.Type == type)
        {
            Advance();
            return true;
        }
        return false;
    }

    private void Expect(TokenType type)
    {
        if (current.Type != type) throw new Exception($"Expected token {type} but got {current.Type} at {current.Position}");
        Advance();
    }

    public QueryRoot Parse()
    {
        QueryFrom? source = null;

        if (current.Type == TokenType.KeywordFrom || current.Type == TokenType.KeywordFor)
        {
            Advance();

            if (current.Type != TokenType.Identifier)
                throw new Exception("Expected table/entity name after 'from' or 'for'");

            string sourceName = current.Lexeme;
            Advance();

            Expression? whereExpr = null;
            if (current.Type == TokenType.KeywordWhere)
            {
                Advance();
                whereExpr = ParseExpression();
            }

            source = new QueryFrom(sourceName, whereExpr);
        }

SkipSourceParse:

        QueryRoot root = current.Type switch
        {
            TokenType.KeywordAdd => WrapMutation(ParseAddStatement(source)),
            TokenType.KeywordRemove => WrapMutation(ParseRemoveStatement(source)),
            TokenType.KeywordUpdate => WrapMutation(ParseUpdateStatement(source)),
            TokenType.KeywordCreate => WrapMutation(ParseCreateTableStatement()),
            TokenType.KeywordDrop => WrapMutation(ParseDropTableStatement()),
            TokenType.KeywordTake => ParseTake(source),
            _ => throw new Exception($"Unexpected token {current.Type} at {current.Position}")
        };

        ParseModifiers(root);

        return root;
    }

    private void ParseModifiers(QueryRoot root)
    {
        while(current.Type is not TokenType.EndOfFile)
        {
            if (current.Type is TokenType.KeywordOrder)
            {
                Advance();
                Expect(TokenType.KeywordBy);
                if (current.Type != TokenType.Identifier) throw new InvalidOperationException("Order by must have an identifier to order on");
                string orderOn = current.Lexeme;
                Advance();
                if (current.Type is TokenType.KeywordDescending)
                {
                    root.Modifiers.Add(new OrderByModifier(orderOn, true));
                    Advance();
                }
                else
                    root.Modifiers.Add(new OrderByModifier(orderOn, false));

            }

            if(current.Type is TokenType.KeywordLimit)
            {
                Advance();
                if (current.Type != TokenType.Number) throw new InvalidOperationException("Limit must be followed by a number");
                root.Modifiers.Add(new LimitModifier(Convert.ToInt32(current.Literal)));
                Advance();
            }
        }
    }
    private QueryRoot WrapMutation(QueryFrom mutation) => new QueryRoot(mutation, null);

    private QueryRoot ParseTake(QueryFrom? source)
    {
        if (source is null)
            throw new InvalidOperationException("Take must have a from clause");
        QueryTake take = ParseTake();
        return new QueryRoot(source, take);
    }

    private QueryTake ParseTake()
    {
        Advance(); // consume Take
        PathExpression? rootPath = null;
        if (current.Type == TokenType.Identifier)
        {
            var segments = new List<PathSegment>
            {
                ParsePathSegment()
            };
            while (Match(TokenType.Arrow))
                segments.Add(ParsePathSegment());

            rootPath = new PathExpression(segments);
        }

        // selection block (must follow)
        SelectionBlock selection = ParseSelectionBlock();

        return new QueryTake(rootPath, selection);
    }

    private PathSegment ParsePathSegment()
    {
        if (current.Type != TokenType.Identifier) throw new Exception("Expected identifier in path segment");
        string field = current.Lexeme;
        Advance();
        FilterBlock? filter = null;
        if (Match(TokenType.LParen))
        {
            var conditions = new List<Expression>();
            while (current.Type != TokenType.RParen && current.Type != TokenType.EndOfFile)
            {
                Expression cond = ParseExpression();
                conditions.Add(cond);
                if (current.Type == TokenType.Comma) Advance();
            }
            Expect(TokenType.RParen);
            filter = new FilterBlock(conditions);
        }
        return new PathSegment(field, filter);
    }

    /**
     
    from people take {
        *,
        work {
            * except {
                internalCode
            }
        }
    }

     */

    private SelectionBlock ParseSelectionBlock(bool isDoingExcept = false)
    {
        Expect(TokenType.LBrace);
        var entries = new List<SelectionEntry>();

        while (current.Type != TokenType.RBrace && current.Type != TokenType.EndOfFile)
        {
            if (current.Type == TokenType.Operator && current.Lexeme == "*")
            {
                Advance();

                if (current.Type == TokenType.KeywordExcept)
                {
                    if (isDoingExcept)
                        throw new InvalidOperationException("Can not except from within an except block!");
                    Advance();
                    var exceptBlock = ParseSelectionBlock(true);
                    entries.Add(new SelectionExcept(exceptBlock));
                }
                else
                {
                    entries.Add(new SelectionEntry("*", null));
                }

                if (current.Type == TokenType.Comma) Advance();
                continue;
            }

            // existing identifier-based selection
            if (current.Type != TokenType.Identifier)
                throw new Exception("Expected identifier, '*' or 'except' in selection block");

            string field = current.Lexeme;
            Advance();

            SelectionBlock? nested = null;
            if (current.Type == TokenType.LBrace)
            {
                nested = ParseSelectionBlock();
            }

            entries.Add(new SelectionEntry(field, nested));

            if (current.Type == TokenType.Comma) Advance();
        }

        Expect(TokenType.RBrace);
        return new SelectionBlock(entries);
    }

    private Expression ParseExpression()
    {
        // support conditional expression: if <cond> then <trueExpr> else <falseExpr>
        if (current.Type == TokenType.KeywordIf)
        {
            Advance(); // consume 'if'
            var condition = ParseExpression();
            Expect(TokenType.KeywordThen);
            var trueExpr = ParseExpression();
            Expect(TokenType.KeywordElse);
            var falseExpr = ParseExpression();
            return new ConditionalExpression(condition, trueExpr, falseExpr);
        }

        // parse left-associative binary expressions (simple precedence: binary operators are same precedence)
        Expression left = ParsePrimary();
        while (current.Type == TokenType.Operator)
        {
            string op = current.Lexeme;
            Advance();
            Expression right = ParsePrimary();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParsePrimary()
    {
        if (current.Type == TokenType.Identifier)
        {
            string ident = current.Lexeme;
            Advance();
            // allow dotted or arrowed access for nested identifiers (e.g. leadDeveloper->studio->country)
            var segments = new List<string> { ident };
            while (current.Type == TokenType.Arrow)
            {
                Advance();
                if (current.Type != TokenType.Identifier) throw new Exception("Expected identifier after -> in expression");
                segments.Add(current.Lexeme);
                Advance();
            }
            if (segments.Count == 1) return new IdentifierExpression(ident);
            // turn into a combined identifier expression using '->' joined path
            return new IdentifierExpression(string.Join("->", segments));
        }

        // handle count(...) and exists(...) as expressions (consume args if present)
        if (current.Type == TokenType.KeywordCount || current.Type == TokenType.KeywordExists)
        {
            string func = current.Lexeme.ToLowerInvariant();
            Advance();
            if (current.Type == TokenType.LParen)
            {
                Advance();
                var inner = ParseExpression();
                Expect(TokenType.RParen);
                return new FunctionCallExpression(func, new List<Expression> { inner });
            }
            return new FunctionCallExpression(func, new List<Expression>());
        }

        if (current.Type == TokenType.Number || current.Type == TokenType.String || current.Type == TokenType.Boolean)
        {
            object? val = current.Literal ?? current.Lexeme;
            Advance();
            return new LiteralExpression(val);
        }
        throw new Exception($"Unexpected token {current.Type} in expression at {current.Position}");
    }

    private CreateTableStatement ParseCreateTableStatement()
    {
        Expect(TokenType.KeywordCreate);
        Expect(TokenType.KeywordTable);

        if (current.Type != TokenType.Identifier) throw new Exception("Expected table name after 'create table'");
        string tableName = current.Lexeme;
        Advance();

        Expect(TokenType.LBrace);
        var fields = new List<TableField>();
        while (current.Type != TokenType.RBrace && current.Type != TokenType.EndOfFile)
        {
            if (current.Type != TokenType.Identifier) throw new Exception("Expected field name in create table block");
            string fieldName = current.Lexeme;
            Advance();

            Expect(TokenType.Colon);

            if (current.Type != TokenType.Identifier) throw new Exception("Expected type name for field");
            string typeName = current.Lexeme;
            Advance();

            bool pk = false;
            if (current.Type == TokenType.Identifier && string.Equals(current.Lexeme, "pk", StringComparison.OrdinalIgnoreCase))
            {
                pk = true;
                Advance();
            }

            fields.Add(new TableField(fieldName, typeName, pk));

            if (current.Type == TokenType.Comma) Advance();
        }
        Expect(TokenType.RBrace);

        return new CreateTableStatement(tableName, fields);
    }

    private DropTableStatement ParseDropTableStatement()
    {
        Expect(TokenType.KeywordDrop);
        Expect(TokenType.KeywordTable);

        if (current.Type != TokenType.Identifier) throw new Exception("Expected table name after 'drop table'");
        string tableName = current.Lexeme;
        Advance();

        return new DropTableStatement(tableName);
    }

    private AssignmentBlock ParseAssignmentBlock()
    {
        Expect(TokenType.LBrace);
        var entries = new List<AssignmentEntry>();

        while (current.Type != TokenType.RBrace && current.Type != TokenType.EndOfFile)
        {
            if (current.Type != TokenType.Identifier)
            {
                throw new Exception("Expected field name in assignment block");
            }

            string field = current.Lexeme;
            Advance();

            Expect(TokenType.Colon); // expect ':'

            Expression value = ParseExpression(); // value expression

            entries.Add(new AssignmentEntry(field, value));

            if (current.Type == TokenType.Comma) Advance();
        }

        Expect(TokenType.RBrace);
        return new AssignmentBlock(entries);
    }

    private AddStatement ParseAddStatement(QueryFrom? source)
    {
        Expect(TokenType.KeywordAdd);
        Expect(TokenType.KeywordTo);

        if (current.Type != TokenType.Identifier)
            throw new Exception("Expected target entity after 'to'");

        string targetName = current.Lexeme;
        Advance();

        // support either a single assignment block or a batch [ { ... }, { ... } ]
        if (current.Type == TokenType.LBracket)
        {
            // batch add
            Advance(); // consume '['
            var blocks = new List<AssignmentBlock>();
            while (current.Type != TokenType.RBracket && current.Type != TokenType.EndOfFile)
            {
                var block = ParseAssignmentBlock();
                blocks.Add(block);
                if (current.Type == TokenType.Comma) Advance();
            }
            Expect(TokenType.RBracket);
            return new AddBatchStatement(
                source?.SourceTypeName,
                source?.Where,
                targetName,
                blocks
            );
        }
        else
        {
            var assignments = ParseAssignmentBlock();
            return new AddStatement(
                source?.SourceTypeName,
                source?.Where,
                targetName,
                assignments
            );
        }
    }


    private RemoveStatement ParseRemoveStatement(QueryFrom? source)
    {
        Expect(TokenType.KeywordRemove);

        if (current.Type != TokenType.Identifier && current.Type != TokenType.KeywordFrom)
            throw new Exception("Expected target entity/table after 'remove'");

        string targetName;
        Expression? whereExpr = null;

        if (current.Type == TokenType.KeywordFrom)
        {
            Advance();
            if (current.Type != TokenType.Identifier)
                throw new Exception("Expected entity name after 'from' in remove statement");
            targetName = current.Lexeme;
            Advance();

            if (current.Type == TokenType.KeywordWhere)
            {
                Advance();
                whereExpr = ParseExpression();
            }
        }
        else
        {
            targetName = current.Lexeme;
            Advance();
            if (current.Type == TokenType.KeywordWhere)
            {
                Advance();
                whereExpr = ParseExpression();
            }
        }

        return new RemoveStatement(
            targetName,
            whereExpr
        );
    }

    private UpdateStatement ParseUpdateStatement(QueryFrom? source)
    {
        Expect(TokenType.KeywordUpdate);
        bool elseAdd = false;

        // optional "or add" continuation: consume "or" "add" if present
        if (current.Type == TokenType.KeywordOr)
        {
            Advance();
            if (current.Type == TokenType.KeywordAdd)
            {
                Advance();
                elseAdd = true;
            }
            else
            {
                throw new Exception("Expected 'add' after 'or' in 'update or add' phrase");
            }
        }

        var updates = ParseAssignmentBlock();
        return new UpdateStatement(
            source?.SourceTypeName,
            source?.Where,
            updates,
            elseAdd
        );
    }
}
