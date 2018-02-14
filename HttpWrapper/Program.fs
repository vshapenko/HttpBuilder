open System.Net.Http
open System
open System.Web
open System.Collections.Specialized

type RequestParameter=
    |Header of (unit->string*string)
    |Content of (unit->HttpContent)
    |Query of (unit->string*string)

type RequestParameters=RequestParameters of RequestParameter list


let setHeader (request:HttpRequestMessage) (header:unit->string*string)=
 let k,v=header()
 request.Headers.Add(k,v)

let setContent (request:HttpRequestMessage) (content:unit->HttpContent)=
  request.Content<-content()

let setQueryParam (col:NameValueCollection) (query:unit->string*string)=
  let k,v=query()
  col.[k]<-v



type RequestBuilder()=
    [<CustomOperation("header")>]
    member this.Header(RequestParameters source,key,value)= RequestParameters[ yield! source 
                                                                               yield Header (fun()->(key,value))]

    [<CustomOperation("query")>]
    member this.Query(RequestParameters source,key,value)= RequestParameters[ yield! source
                                                                              yield  Query (fun()->(key,value))]

    [<CustomOperation("content")>]
    member this.Content(RequestParameters source,content)= RequestParameters[ yield! source
                                                                              yield Content (fun()->content)]

    member x.Yield (()) = RequestParameters []


let create method (url:string) builder=
   let message= new HttpRequestMessage(method,url)
   let requestBuilder=new UriBuilder(message.RequestUri)
   let col=HttpUtility.ParseQueryString(requestBuilder.Query)
   match builder with
    |RequestParameters l->l|>List.fold(fun acc x->match x with
                                                  |Header f->setHeader acc f
                                                  |Content c->setContent acc c
                                                  |Query q->setQueryParam col q
                                                  acc) message |>ignore
   requestBuilder.Query<-col.ToString()
   message.RequestUri<-requestBuilder.Uri
   message
   

let http=RequestBuilder()

let Get url builder=create HttpMethod.Get url builder
let Post url builder =create HttpMethod.Post url builder
let Put url builder=create HttpMethod.Put url builder
let Delete url builder=create HttpMethod.Delete url builder
let Options url builder =create HttpMethod.Options url builder

 
[<EntryPoint>]
let main argv = 
    let b=http{
                header "a" "b"     
                header "c" "d"  
                query "1" "2"
               } 
    let request =Get "http://lenta.ru" b                                         
    let client=new HttpClient()
    let data=client.SendAsync(request).Result.Content.ReadAsStringAsync().Result

    0 // return an integer exit code
