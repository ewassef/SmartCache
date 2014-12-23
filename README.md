SmartCache
==========
A smart distribute wrapper with notification

Traditionally, a distributed cache usually offloads the cache to a BHS (Big Honkin Server) and all the nodes request their cache from that server (or cluster). 
This is tried and tested, but will usually cost you tremendously in serialization and deserialization. I've seen this process abused more than benefitted from especially when the object graph starts becoming pretty large or if the data lookup starts getting 'fuzzy' (ie. Not just a key) This is exasperated by JSON or BSON making the cache-savior, your bottleneck.

This project is intended to attempt (note the use of the word attempt) to resolve these issues for specific cases. 

The principle is to keep the cache on-board and communicate with the others nodes when an item becomes stale so they dump their copy, if they have one. 

Furthermore, this on board cache can now keep a single copy of the object even if its used in other collections, this keeps the memory footprint TINY. basically, to store a list of an item, we store the key to it and keep the original copy in the item cache underneath.
