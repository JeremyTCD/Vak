# Vector Art Kit Web API

## Description
Web API for Vector Art Kit

## Architecture
Every end point is defined by an action. 

### Role Of An Action
Actions serve as routers. Request parameters must be passed on to a service that can handle the request. Services
must return simple enum results. Actions must then convert these results into responses. This pattern makes it
easy to reuse logic contained in services. For example, only the actions would need to be changed to convert 
an api into a fully fledged razor application.

### Form/Action Models
Actions with parameters must have either a request model wrapping the parameters. These models facilitate strong typing in 
tests and client side operations. For example, request models are copied to client projects to facilitate strongly typed creation of requests.

### Response Models
Each action has a response model. Response models implement the IErrorResponseModel interface. This enforces a consistent 
shape for responses from all actions. Also, response models are copied to client projects to facilitate strongly 
typed handling of responses by web client projects.

## Testing Methodology
All actions must be integration tested. This is necessary since significant aspects of action logic are defined using annotations and
other forms of configuration. For example, the ValidateAntiForgeryAttribute and the HttpPostAttribute attributes define critical behaviours 
that must be tested for.


