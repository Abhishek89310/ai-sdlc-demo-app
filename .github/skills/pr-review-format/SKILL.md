---
name: pr-review-format
description: Fixed output format for code review findings so reviews are consistent and can be pasted into a pull request.
---

# Review output format

Print one block per finding, nothing else around them:

```
### F<number> · <severity: high|medium|low> · <category>
- Where: <path>:<line>
- Issue: <one sentence>
- Why it matters: <one sentence with a concrete failing input>
- Suggestion: <one sentence>
- Verdict: <fixed | dismissed: reason | deferred: reason>   <- filled in by the author
```

If there are no findings print `No findings.` Keep the order most severe first.
