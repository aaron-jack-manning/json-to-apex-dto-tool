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
    | Null ->
        failwith "swapNulls function should have been called before jsonValidator. if this is the case then swapNulls has likely failed"
    | _ ->
        allSameType
| _ ->
    true




