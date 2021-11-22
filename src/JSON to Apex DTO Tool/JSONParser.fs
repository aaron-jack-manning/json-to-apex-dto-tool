module JSONParser

open System
open FParsec

type JSONValue =
    | String of string
    | Number of float
    | Bool of bool
    | Null
    | Object of Map<string, JSONValue>
    | Array of JSONValue list

let nullParser : Parser<_, unit> =
    pstring "null"
    >>% Null

let boolParser : Parser<_, unit> =
    let trueParser : Parser<_, unit> =
        pstring "true"
        >>% (Bool true)

    let falseParser : Parser<_, unit> =
        pstring "false"
        >>% (Bool false)

    trueParser <|> falseParser

let stringParser : Parser<_, unit> =
    let unescapedCharacterParser : Parser<_, unit> =
        satisfy (fun character -> character <> '\\' && character <> '\"')
    
    let escapedCharacterParser : Parser<_, unit> =
        [
            ("\\\"",'\"')
            ("\\\\",'\\')
            ("\\/",'/')
            ("\\b",'\b')
            ("\\f",'\f')
            ("\\n",'\n')
            ("\\r",'\r')
            ("\\t",'\t')
        ]
        |> List.map
            (
                fun (escapedCharacter, correspondingCharacter) ->
                    pstring escapedCharacter
                    |>> (fun x -> correspondingCharacter)
            )
        |> choice
    
    let unicodeCharacterParser : Parser<_, unit> =
        let prefixParser =
            pstring "\\u"
    
        let hexDigitParser =
            anyOf (['0'..'9'] @ ['A'..'F'] @ ['a'..'f'])
    
        let hexCharactersParser =
            tuple4 hexDigitParser hexDigitParser hexDigitParser hexDigitParser
    
        let hexCharactersToCharacter (char1, char2, char3, char4) =
            let hexCharactersString = sprintf "%c%c%c%c" char1 char2 char3 char4
    
            (hexCharactersString, Globalization.NumberStyles.HexNumber)
            |> Int32.Parse
            |> char
    
        prefixParser >>. hexCharactersParser
        |>> hexCharactersToCharacter
    
    let quoteParser : Parser<_, unit> =
        pchar '\"'

    quoteParser >>. manyChars (unescapedCharacterParser <|> escapedCharacterParser <|> unicodeCharacterParser) .>> quoteParser

let stringValueParser =
    stringParser |>> String

let numberParser : Parser<_, unit> =
    numberLiteral (NumberLiteralOptions.DefaultFloat ||| NumberLiteralOptions.DefaultInteger ||| NumberLiteralOptions.DefaultUnsignedInteger) "number"
    |>> (fun x -> Number (float x.String))

let primitiveParser : Parser<_, unit> =
    nullParser <|> boolParser <|> stringValueParser <|> numberParser


// Common array and object parsers
let commaParser = pchar ',' .>> spaces

// Array and Object parser references
let objectParser, objectParserImpl = createParserForwardedToRef()
let arrayParser, arrayParserImpl = createParserForwardedToRef()

// Object parser start
let openBraceParser = spaces >>. pchar '{' .>> spaces
let closeBraceParser = pchar '}' .>> spaces
let colonParser = pchar ':' .>> spaces
    
let keyParser = stringParser .>> spaces .>> colonParser .>> spaces
let valueParser = (primitiveParser <|> objectParser <|> arrayParser) .>> spaces //  <|> objectParser ()

let keyValueParser = keyParser .>>. valueParser

let keyValuePairList =
    sepBy keyValueParser commaParser
    |>> Map.ofList
    |>> Object

objectParserImpl := spaces >>. (openBraceParser >>. keyValuePairList .>> closeBraceParser) .>> spaces
// Object parser end

// Array parser start
let openBracketParser = pchar '[' .>> spaces
let closeBracketParser = pchar ']' .>> spaces

let arrayArgumentListParser =
    sepBy (objectParser <|>  primitiveParser) commaParser
    |>> Array

arrayParserImpl := spaces >>. (openBracketParser >>. arrayArgumentListParser .>> closeBracketParser) .>> spaces

let jsonParser =
    spaces >>. (arrayParser <|> objectParser)


let parseJsonString inputJson =
    let parsedResult = run jsonParser inputJson

    match parsedResult with
    | Success (result, userState, position) ->
        result
    | Failure (message, parserError, userState) ->
        failwith message