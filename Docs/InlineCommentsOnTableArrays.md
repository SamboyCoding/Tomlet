## Inline Comments on Table-Arrays
### A conundrum

Tomlet supports two kinds of comments on TOML values: inline comments and preceding comments.

Preceding comments are simple to place in the file - they always go on a separate line to the value, before it,
and can be multiline by virtue of this.

Inline comments are placed on the same line as the value, immediately after it, and cannot be multiline.
Again, for most values, this is relatively easy to do.

For simple, primitive key-value pairs, one-line arrays, or one-line inline tables,
the resulting structure is something like this:
```toml
# preceding
# comment
# goes here
myKey = "myValue" # inline comment
```

For tables, it looks like this (a table will never be made inline if it contains any kind of comment):
```toml
# preceding comment
[myTable] # inline comment
myKey = "myValue"
```

For inline arrays, it's a little bit messier in the event that Tomlet decides to wrap the array
over multiple lines (e.g. if it contains a nested inline table, or any comments on the individual items),
but it still makes sense:
```toml
# preceding comment on the array
array = [
    # preceding comment on the first element
    "myValue", # inline comment on the first element
    { a = "b", one = 2 }, # inline comment on the second element which is an inline table
] # inline comment on the array itself
```

However, a problem arises if we consider the case of inline comments on table-arrays 
(that is, arrays of tables that cannot be represented inline):
```toml
# preceding comment on the table-array

# preceding comment on the first table
[[myTableArray]] # inline comment space, but for what?
myKey = "myValue"

[[myTableArray]] # more inline comment space.
myKey = "myValue2"
```
There are two locations here that we could place inline comments, namely both table-array headers.

Each could be used either as the inline comment on the table at a given index, or as the inline comment
on the table-array itself. However, using it for both could and probably would get confusing.

Therefore, I've made the decision that this slot is used by the inline comments on the elements 
of the table-array - that is, the tables themselves. This means that the inline comments on the table-array
will result in an exception, which may well be what linked you to this document.

If you are in a situation where you wanted an inline comment on the table-array itself, please consider
using a preceding comment, but if you really want to simulate an inline comment, I suggest simply
setting a comment on the first table and leaving the rest blank.