## Jering.VectorArtKit.Indexer
This project extracts data from Jering.VectoraArtKit.Database and indexes data into ElasticSearch.

### Why use a standalone service over indexing directly from the application layer?
- Maintain consistency with the SSOT
  - If ES indexing was done in the application layer, every SSOT update and its corresponding ES indexing would need to be
    executed as an atomic transaction. Implementing such a pattern would introduce tight coupling between the application and
    the search engine. It would also affect user experience since failed ES updates will require SSOT rollbacks. Indexing through
    the indexer facilitates multiple retries and even recovery measures if ES goes out of sync.
- Avoid large numbers of Http request for high frequency updates
  - For example, if every VakUnit has a page view counter, ES indexing in the application layer would mean a large number of 
    Http requests. Polling the SSOT for changes and using the ES bulk Api for ES indexing negates this issue.
  - The official ES documentation recommends using the bulk Api over numerous individual requests.
- Establishes a pattern that makes it easy to add additional services
  - For example, if caching from the SSOT is required, this project can be expanded on to index data into the cache. The alternative
    would be to cache directly from the application. This would require changes throughout the application. Adding transform and load logic for multiple services will 
    clutter application code.
- Indexing directly from the application can reduce latency. However, ES has a refresh interval that is 1 second by default.
  Refreshing is expensive as it involves creating new indices. It is for this reason that it cannot occur on every request. Extracting data 
  from the SSOT, indexing it in ES and manually refreshing every second means the latency from using this project should not exceed the
  latency realistically achievable from indexing directly form the application. 
   
### Options

### Future Improvements
- Is selecting from Sql Server as Json more efficient than joining client side?
- Support high availability
  - Distributed system logic
- Sql Server rowverion used for versioning. Each rowversion value is an 8 byte unsigned integer. It can be cast to bigint but will not 
  be valid after a certain point since bigint is signed. This point is unlikely to be reached but a workaround could prevent a 
  catastrophe.