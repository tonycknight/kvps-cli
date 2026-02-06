namespace kvps

open System
open System.Linq

module Math =
  let max (x: int) (y: int) = Math.Max(x, y)
  let min (x: int) (y: int) = Math.Min(x, y)

module Strings =
  let join (delimiter: string) (values: seq<string>) = String.Join(delimiter, values)

  let stars (value: string) = new string ('*', value.Length)

  let trim (value: string) = value.Trim()

  let notEmpty (value: string) = String.IsNullOrWhiteSpace(value) |> not

  let trimSeq (values: seq<string>) =
    values |> Seq.map trim |> Seq.filter notEmpty

  let padR length (value: string) =
    let pad = length - value.Length |> Math.max 0
    new String(' ', pad) |> sprintf "%s%s" value

  let padRSeq maxLength (values: seq<string>) = values |> Seq.map (padR maxLength)

  let maxLength (values: seq<string>) =
    values
    |> Seq.map (fun s -> s.Length)
    |> Seq.fold (fun longest x -> if longest > x then longest else x) 0

  let green (value: string) = Crayon.Output.Bright.Green(value)

  let red (value: string) = Crayon.Output.Bright.Red(value)

  let cyan (value: string) = Crayon.Output.Bright.Cyan(value)

  let yellow (value: string) = Crayon.Output.Bright.Yellow(value)

  let bytes (value: string) = System.Text.Encoding.UTF8.GetBytes(value)

  let fromBytes (data: byte[]) = System.Text.Encoding.UTF8.GetString(data)

  let toBase64 (data: byte[]) = System.Convert.ToBase64String(data)

  let fromBase64 (value: string) = System.Convert.FromBase64String(value)

module Bool =
  let toRc =
    function
    | true -> 0
    | false -> 2

module Io =
  open System.IO

  let filePath folder fileName =
    let root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    let dir = Path.Combine(root, folder)

    if Directory.Exists(dir) |> not then
      Directory.CreateDirectory(dir) |> ignore

    Path.Combine(dir, fileName)

  let dataFilePath = filePath "kvps-data"

  let resolvePath (folder: string) =
    if Path.IsPathRooted(folder) then
      folder
    else
      let workingPath = Directory.GetCurrentDirectory()
      Path.Combine(workingPath, folder)

  let writeFile (filePath: string) (contents: string) =
    System.IO.File.WriteAllText(filePath, contents)

  let readFile filePath = System.IO.File.ReadAllText(filePath)

module Console =
  let writeLine (value: string) = Console.Out.WriteLine(value)

  let writeLines (values: seq<string>) = values |> Seq.iter writeLine

module Clipboard =
  let set (value: string) = TextCopy.ClipboardService.SetText value

module Option =
  let nullToOption (value) =
    if Object.ReferenceEquals(value, null) then
      None
    else
      Some value

  let nullToDefault defaultValue value =
    value |> nullToOption |> Option.defaultValue defaultValue

module Reflection =

  let getAsm () =
    System.Reflection.Assembly.GetExecutingAssembly()

  let getAttrs (asm: System.Reflection.Assembly) =
    asm.GetCustomAttributes(true).OfType<Attribute>() |> Seq.map id

  let getAttrValue<'a when 'a :> Attribute> (f: 'a -> string) (attrs: seq<Attribute>) =
    attrs.OfType<'a>().FirstOrDefault() |> Option.nullToOption |> Option.map f

  let getVersionValue attrs =
    attrs
    |> getAttrValue<System.Reflection.AssemblyInformationalVersionAttribute> (fun a -> a.InformationalVersion)

  let getCopyrightValue attrs =
    attrs
    |> getAttrValue<System.Reflection.AssemblyCopyrightAttribute> (fun a -> a.Copyright)

module Seq =
  let flattenSomes (values: seq<'a option>) =
    values |> Seq.filter Option.isSome |> Seq.map Option.get

module LiteDb =
  open LiteDB

  [<Literal>]
  let pw = "4737c58a-8a21-424d-bb88-da16a192ec07" // Filthy hack, we're just obfuscating files here

  let filePath dbName =
    dbName |> sprintf "%s.db" |> Io.dataFilePath

  let connection path =
    sprintf "Filename=%s;Password=%s" path pw

  let db (cn: string) = new LiteDatabase(cn)

  let collection<'a> (name: string) (db: ILiteDatabase) = db.GetCollection<'a>(name)

module Encryption =
  open System.IO
  open System.Security.Cryptography
    
  let private encryptByEncryptor encryptor (value: string) =
      use outStream = new MemoryStream()
                  
      use cryptStream = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write)
      use cryptWriter = new StreamWriter(cryptStream)
      cryptWriter.Write value
      cryptWriter.Flush()
      cryptStream.FlushFinalBlock()
      
      outStream.ToArray()

  let private key (aes: Aes) (password: string) =
    let key = Strings.bytes password
    if aes.ValidKeySize key.Length then      
      key
    else
      let key = Array.init<byte> aes.Key.Length (fun _ -> 0uy)

      let pw = Strings.bytes password
      if pw.Length > key.Length then
        failwith "The password is too big."

      Array.blit pw 0 key 0 pw.Length
      key

  let private readIv (aes: Aes) (stream: Stream) =
    let iv = Array.init<byte> aes.IV.Length (fun _ -> 0uy)
    let mutable numBytesToRead = aes.IV.Length
    let mutable numBytesRead = 0
    
    while numBytesToRead > 0 do 
      let n = stream.Read(iv, numBytesRead, numBytesToRead)
      numBytesRead <- numBytesRead + n 
      numBytesToRead <- numBytesToRead - n

    iv

  let encrypt (password: string) (value: string) =    
    use aes = Aes.Create()
    
    let key = password |> key aes
    let iv = aes.IV
    
    use encryptor = aes.CreateEncryptor(key, iv)

    let data = encryptByEncryptor encryptor value
    
    data |> Array.append iv
    
  let decrypt (password: string) (data: byte[]) =
    use aes = Aes.Create()
    let key = password |> key aes
    
    use inStream = new MemoryStream(data)
    
    let iv = inStream |> readIv aes
      
    use decryptor = aes.CreateDecryptor(key, iv)
    
    use cryptoStream = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read)

    use cryptReader = new StreamReader(cryptoStream)
    cryptReader.ReadToEnd()