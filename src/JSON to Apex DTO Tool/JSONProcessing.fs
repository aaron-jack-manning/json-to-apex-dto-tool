module JSONProcessing

open System.Text.RegularExpressions
open Microsoft.FSharp.Reflection

open JSONParser

let rec swapNulls = function
    | Object jsonObject ->
        [
            for keyValuePair in Map.toList jsonObject do
                let key, value = keyValuePair

                yield (key, swapNulls value)
        ]
        |> Map.ofList
        |> Object
    | Array jsonArray ->
        jsonArray
        |> List.map (fun x -> swapNulls x)
        |> Array
    | Null ->
        Object Map.empty
    // For JSON primitives
    | x -> x


type Validation =
    | Valid
    | Invalid of string

let rec jsonValidation : (JSONValue -> Validation) = function
    | Object jsonObject ->

        let extractMessage = function
            | Valid -> ""
            | Invalid message -> message
    
        let errorString =
            [
                for keyValuePair in Map.toList jsonObject do
                    let key, value = keyValuePair

                    yield jsonValidation value
            ]
            |> List.fold (fun s x -> s + (extractMessage x)) ""  

        


        if errorString |> String.length = 0 then
            Valid
        else
            Invalid errorString

    | Array jsonArray ->
        if jsonArray |> List.length = 0 then
            Invalid "a json array must have a length of at least one in the sample"
        else
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

                if allLong || allDouble then
                    Valid
                else
                    Invalid "all numerical types within an array must be the same (long or float)" 
            | Array jsonArray ->
                Invalid "json does not permit arrays within arrays"
            | Null ->
                Invalid "swapNulls function should have been called before jsonValidator. if this is the case then swapNulls has likely failed"
            | _ ->
                if allSameType then
                    Valid
                else
                    Invalid "all types within an array must be the same"
    | _ ->
        Valid




