open System.IO
open System

let getProperty (prop:string) (s:string) : string =
    let prop = prop + ": " 
    s.IndexOf(prop) + prop.Length |> s.Substring

let getGuid = getProperty("guid")

[<StructuredFormatDisplay("{oldGuid} => {newGuid}")>]
type guidAssosiation(old : string, newId : string) =
     member this.oldGuid = old  
     member this.newGuid = newId

let stripchars (chars:string) (str:string) :string =
  Seq.fold
    (fun (str: string) chr ->
      str.Replace(chr |> string, "").Replace(chr |> string, ""))
    str chars

    
let createNewGuidAssosiation (old:string) : guidAssosiation =
    guidAssosiation(old, Guid.NewGuid().ToString() |> stripchars("-") |> String.Concat)

let transformString (str:string) (oldStr:string) (newStr:string) :string = 
    str.Replace(oldStr, newStr)


[<EntryPoint>]
let main args = 
    let pathList = File.ReadAllLines(args.[0]) |> Seq.toList

    let guidPairs = pathList.Tail
                    |> List.map (fun x ->
                        let content = File.ReadAllLines(x) |> Seq.toList
                        let guidPair = [for line in content do
                                        if line.Contains("guid:") then 
                                            yield getGuid line]
                                        |> List.head
                                        |> createNewGuidAssosiation
                        let newContent = content
                                        |> List.map(fun line -> transformString line guidPair.oldGuid guidPair.newGuid)
 
                        File.WriteAllLines(x, newContent)
                        guidPair)

    let transformedScene = File.ReadAllLines(pathList.Head) 
                           |> Seq.toList
                           |> List.map(fun line -> 
                                guidPairs 
                                |> List.fold (fun line pair ->
                                transformString line pair.oldGuid pair.newGuid) line)
                             
    let writeScene =  File.WriteAllLines(pathList.Head, transformedScene)
   
    printfn "%A" guidPairs
    0
