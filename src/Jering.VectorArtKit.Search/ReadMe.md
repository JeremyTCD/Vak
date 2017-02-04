## Jering.VectorArtKit.Search
This project contains configurations and mappings for ElasticSearch.
It serves a similar function to Jering.VectorArtKit.Database. Files within
this project are the ElasticSearch equivalent to a relational database 
schema.

### Why use a standalone project?
This project facilicates version control of the ElasticSearch schema.

### Common operations

#### Delete an index
curl -XDELETE "localhost:9200/_all"

#### Create an index with custom mapping
curl -XPUT "localhost:9200/vak_units_v1" --data "@vak_units_v1_mapping.json"

#### List all indices
curl -XGET "localhost:9200/_cat/indices?v&pretty"

#### Assign alias
curl -XPUT "localhost:9200/vak_units_v1/_alias/vak_units"

#### Get aliases
curl -XGET "localhost:9200/_all/_alias/*?pretty"

#### Insert a document
curl -XPUT "localhost:9200/vak_units/vak_unit/1" --data "{\"name\": \"Test\", \"accountId\":  1,\"tags\":  [\"a\",\"b\"]}"