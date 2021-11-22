module ApexDTO

open JSONParser

open System
open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection

let rec jsonValidation = function
    | Object jsonObject ->
        [
            for keyValuePair in Map.toList jsonObject do
                let key, value = keyValuePair

                yield jsonValidation value
        ]
        |> List.forall (fun x -> x)
    | Array jsonArray ->
        let unionCaseName (discriminatedUnion : 'a) =
            match FSharpValue.GetUnionFields(discriminatedUnion, typeof<'a>) with
            | case, _ -> case.Name
        let baseCaseName
            = unionCaseName jsonArray.[0]
        let allSameType =
            (true, jsonArray)
            ||> List.fold (fun state element -> state && (unionCaseName element = baseCaseName))

        match jsonArray.[0] with
        | Number jsonNumber ->
            let allLong =
                jsonArray
                |> List.forall
                    (
                        fun x ->
                            match x with
                            | Number jsonNumber2 ->
                                truncate jsonNumber2 = jsonNumber2
                            | _ ->
                                failwith "this error should never occur"
                    )
            let allDouble =
                jsonArray
                |> List.forall
                    (
                        fun x ->
                            match x with
                            | Number jsonNumber2 ->
                                truncate jsonNumber2 <> jsonNumber2
                            | _ ->
                                failwith "this error should never occur"
                    )
            allLong || allDouble
        | Array jsonArray ->
            failwith "json does not permit arrays within arrays"
        | _ ->
            allSameType
    | Null ->
        failwith "try again without null values, as type cannot be inferred"
    | _ ->
        true

let inline indent counter =
    let space = ' '
    let spaceArray = Array.zeroCreate (counter * 4)
    Array.Fill (spaceArray, space)

    spaceArray |> System.String |> string

let generateParseJsonMethod (className : string) : string =
    
    [
        indent 1 + $"public static " + className + "DTO FromJSON(string jsonString)\n";
        indent 1 + "{\n";
        indent 2 + "return (" + className + "DTO)JSON.Deserialize(jsonString, " + className + "DTO.Class);\n";
        indent 1 + "}\n"
    ] |> List.fold (fun state element -> state + element) ("")

let insertionIndexCalculator (topLevelClass : string) : int =
    topLevelClass.IndexOf("{") + 1


let apexDtoGenerator (className : string) (jsonValue : JSONValue) =
    let mutable objectList = List.empty
    let mutable unnamedClassCount = -1
    let mutable currentArrayKey = ""

    let rec dtoBuilder = function
        | Object jsonObject ->
            let mutable objectString = ""
        
            for keyValuePair in Map.toList jsonObject do
                let key, value = keyValuePair

                // this is to set the array key to identify the name for the anonymous objects in an array
                match value with
                | Array jsonArray ->
                    currentArrayKey <- key + "Single"
                | _ -> ()

                let apexValue = dtoBuilder value

                match value with
                | Object internalObjectJson ->
                    //objectString <- objectString + indent (indentation) + "class " + key + "DTO\n" + indent indentation + "{\n" + apexValue + indent indentation + "}\n"
                    
                    objectList <- List.append objectList ["public class " + key + "DTO\n" + "{\n" + apexValue + "}"]

                    objectString <- objectString + indent 1 + $"public {key}DTO {key};\n"
                | Array jsonArray ->
                    objectString <- objectString + $"{apexValue}{key};\n"
                | _ ->
                    objectString <- objectString + $"{apexValue}{key};\n"

            objectString
        | String jsonString ->
            indent 1 + "public string "
        | Number jsonNumber ->
            indent 1 +
                if truncate jsonNumber = jsonNumber then
                    "public long "
                else
                    "public double "
        | Bool jsonBool ->
            indent 1 + "public boolean "
        | Array jsonArray ->
            match jsonArray.[0] with
            | String jsonString ->
                indent 1 + "public List<string> "
            | Number jsonNumber ->
                let allLong =
                    jsonArray
                    |> List.forall
                        (
                            fun x ->
                                match x with
                                | Number jsonNumber2 ->
                                    truncate jsonNumber2 = jsonNumber2
                                | _ ->
                                    failwith "this error should never occur"
                        )

                if allLong then
                    indent 1 + "public List<long> "
                else
                    indent 1 + "public List<double> "
            | Bool jsonBool ->
                indent 1 + "public List<boolean> "
            | Object jsonObject ->
                let apexValue = dtoBuilder (JSONValue.Object jsonObject)

                unnamedClassCount <- unnamedClassCount + 1

                objectList <- List.append objectList ["class " + string currentArrayKey + "DTO\n{\n" + apexValue + "}"]

                indent 1 + $"public List<{currentArrayKey}DTO> "
            | _ ->
                failwith "this error should never occur"
        | _ ->
            failwith "this error should never occur"

    let topLevelClass =
        match jsonValue with
        | Array jsonArray ->
            "public class " + className + "DTO\n{\n" + dtoBuilder jsonArray.[0] + "}"

        | Object jsonObject ->
            "public class " + className + "DTO\n{\n" + dtoBuilder (JSONValue.Object jsonObject) + "}"
        | _ ->
            failwith "top level of json must be an array or object"

    let insertionIndex = insertionIndexCalculator topLevelClass
    
    let otherClasses =
        ("", objectList)
        ||> List.fold (fun state element -> state + element + "\n")
    
    let allClassesInTopLevel =
        if String.length otherClasses = 0 then
            topLevelClass.Insert(insertionIndex, Regex.Replace(otherClasses, "\n", "\n    "))
        else
            topLevelClass.Insert(insertionIndex, "\n" + indent 1 + Regex.Replace(otherClasses, "\n", "\n    "))

    allClassesInTopLevel.Insert(insertionIndex, "\n" + generateParseJsonMethod className)

let jsonToApexDto (className : string) (jsonString : string) =
    let parsedJson = parseJsonString jsonString

    jsonValidation parsedJson |> ignore
    
    apexDtoGenerator className parsedJson
    