open System.Net.Http
open System
type RequestBuilder (method:HttpMethod,url:string,content:(unit->HttpContent) option)=
      member this.Delay(f)=f()

      [<CustomOperation("header")>]
      member this.Header(f:HttpRequestMessage->HttpRequestMessage,name:string,value:string)=
        f>>fun r->
           r.Headers.Add(name,value)
           r 
      [<CustomOperation("content")>]
      member this.Content(f:HttpRequestMessage->HttpRequestMessage,content:unit->HttpContent)=
         f>> fun r->
            r.Content<-content()
      member this.Yield (())=fun (r:HttpRequestMessage)->   
                              r.Method<-method
                              r.RequestUri<-new Uri(url)
                              r


let create method url content=RequestBuilder(method,url,content)

let Get url=create HttpMethod.Get url None
let Post url f =create HttpMethod.Post url (Some f)
let Put url=create HttpMethod.Put url
let Delete url=create HttpMethod.Delete url None

       
[<EntryPoint>]
let main argv = 
    let request =Get "http://lenta.ru" {
                                        header "h1" "v1"
                                        header "h2" "v2"
                                       }

    0 // return an integer exit code
