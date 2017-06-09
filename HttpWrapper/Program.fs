open System.Net.Http
open System
type RequestBuilder()=    
      member this.Combine (a,b) = a>>b
      member this.Yield (())=fun r->r
      member this.Yield (f)=f
      member this.Delay(f)=f()

      [<CustomOperation("method")>]
      member this.Method(f:HttpRequestMessage->HttpRequestMessage,m:HttpMethod)=
        f>>fun r->   
           r.Method<-m
           r

      [<CustomOperation("header")>]
      member this.Header(f:HttpRequestMessage->HttpRequestMessage,name:string,value:string)=
        f>>fun r->
           r.Headers.Add(name,value)
           r
      [<CustomOperation("url")>]
      member this.Url (f:HttpRequestMessage->HttpRequestMessage,uri)=
       f>>fun r->
           r.RequestUri<-new Uri(uri)
           r    
       
[<EntryPoint>]
let main argv = 
    let req  =RequestBuilder()
    let getBuilder=req {method HttpMethod.Get}
    let headers= req{
                     header "Authorization" "Basic"
                     header "MyHeader" "MyValue"
                   }

    let r1=req{ 
                 yield getBuilder
                 yield headers
              }
    let r2=async{
                   return req{
                        header "Authorization" "Basic"
                        header "MyHeader" "MyValue"
                       }
                }
    printfn "%A" argv
    let r2=new HttpRequestMessage()|>r1
    0 // return an integer exit code
