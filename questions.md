# Questions

## How did you approach solving the problem?
> I started by analyzing the requirements in the README. Once I believed I understood the requirements, I then did research. I found a few examples of how to process files.  
<ol>
    <li> Get the file download working without cancellation. </li>
    <li> Get the partial download working without cancellation. </li>
    <li> Noted the poor performance of single thread partial download in TODO. </li>
    <li> Implemented cancellation tokens. </li>
    <li> Implemented retry and backoff. </li>
    <li> Created Mock WebCaller for Testing. </li>
    <li> Implemented Tests to verify solution.</li>
    <li> Added more error handling. </li>
</ol>


## How did you verify your solution works correctly?
> A combination of testing strategies, I started with manual testing, I then implemented mocks to allow for integration testing. 


## How long did you spend on the exercise?
> About 5 hours spread across the holiday weekend and tuesday. 



## What would you add if you had more time and how?
> More test coverage is needed, multi-threading for partial download, file content verification, time estimation.


