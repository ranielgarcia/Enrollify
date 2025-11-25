# Question

In this .NET Vogen library documentation https://stevedunn.github.io/Vogen/efcoreintegrationhowto.html when overriding the ConfigureConventions and use the RegisterAllInVogenEfCoreConverters, does that mean I don't have to specify the HasConversion on each entity configuration?

## Answer

Good question. Short answer: yes, using RegisterAllInVogenEfCoreConverters() can eliminate the need to manually call HasConversion for each Vogen value object — but there are some caveats to be aware of.

### What the docs say

From the Vogen EF-Core integration docs:

* You can generate EF Core converters for your VOs either by placing [ValueObject(..., conversions: Conversions.EfCoreValueConverter)] on each VO, or by marking them in a marker class using [EfCoreConverter<…>]. 
stevedunn.github.io

* Once converters are generated, you have two ways to wire them into EF:

    1. Explicitly in OnModelCreating — call .HasConversion(new YourVo.EfCoreValueConverter()) on each property. 
stevedunn.github.io

    2. Convention-based (if on .NET 8+) — override ConfigureConventions(...) in your DbContext and call configurationBuilder.RegisterAllInVogenEfCoreConverters(). 
stevedunn.github.io

* That RegisterAllInVogenEfCoreConverters is a generated extension method, so it picks up all the EF Core converters that your marker class defined. 
stevedunn.github.io

### How EF Core Conventions Work

Some background on how ConfigureConventions plays into EF Core’s model building:

* ConfigureConventions is part of EF Core’s pre-convention configuration. It runs before things like OnModelCreating. 
Microsoft Learn
+1

* By adding a convention for a type, you instruct EF Core to apply that value converter automatically to all properties of that CLR type (unless explicitly overridden). 
Microsoft Learn

* So, when Vogen generates converters and you register them via RegisterAllInVogenEfCoreConverters(), EF Core can automatically apply those converters for matching VO types.

### Implications & Caveats

* You don’t have to call HasConversion on every property: If the types match (i.e., your entity’s property type is one of your Vogen value-object types for which a converter was generated), the convention will apply the correct converter without further configuration.

* Explicit config still wins: If you explicitly call HasConversion(...) in OnModelCreating, that will take precedence over the convention-based one. (This is generally how EF Core works: more specific configuration overrides conventions.)

* Marker class naming matters: The generated RegisterAllInVogenEfCoreConverters method’s name depends on your marker class (VogenEfCoreConverters in docs). If you name it differently or have more than one marker class, you need to call the corresponding register method. 
stevedunn.github.io

* You must have generated converters: This only works if you’ve properly marked your VOs (or used the marker class) so that Vogen actually generates the EF Core converters/comparers. If you forgot to set up the conversions, the convention method has nothing to register.

* Version requirement: The docs mention this pattern “if you're using .NET 8 or greater” for ConfigureConventions technique. 
stevedunn.github.io
 (Make sure your EF Core version supports pre-convention configuration in the way you expect.)

### Conclusion & Recommendation

* Yes, you can skip manual HasConversion(...) on every property if you use RegisterAllInVogenEfCoreConverters() in ConfigureConventions.

* That’s a cleaner, DRYer way to apply your Vogen value-object converters across your entire model.

* But double-check that:

1. Vogen is generating those EF Core converters for all your VOs.

2. Your DbContext’s ConfigureConventions override is correctly calling the generated register method.

3. You don’t have special cases where a different conversion or behavior is needed — then you might still want to use HasConversion for those properties.

## Question 2

What about the HasVogenConversion(), should I still use it along side with RegisterAllInVogenEfCoreConverters?

Short answer:
No — you normally should NOT use HasVogenConversion() if you are already using RegisterAllInVogenEfCoreConverters().
Pick one approach for consistency, unless you have a very specific override scenario.

---

### ✅ Why you don’t need HasVogenConversion() when using RegisterAllInVogenEfCoreConverters()

RegisterAllInVogenEfCoreConverters() registers generated value converters globally at the convention level in EF Core.
This means:

* Every property whose CLR type is a Vogen value object
* Automatically gets the correct EF Core converter
* Without needing any per-property configuration
* As long as the VO has a generated EF-Core converter (via attribute or marker class)

So EF Core handles the conversion behind the scenes. No need to repeat HasVogenConversion().

### 🔍 When would you still use HasVogenConversion()?

Only if you want to override the global convention for a specific property:

Example cases

* You want a custom converter for one property only
* You want to change the column type (e.g., map value object to varchar(30) for one table)
* You need specific behavior during inserts/updates

In those rare cases, using HasVogenConversion() overrides the convention.

### 🧠 Rule of Thumb
Approach	                            When to Use	                            Notes
RegisterAllInVogenEfCoreConverters()	Standard/default for whole DbContext	Best for large apps, DRY, automatic
HasVogenConversion()	                Per-property override	                Only if you need something different from the convention


### 👍 Recommended Best Practice

For modern .NET 8+ apps using Vogen:

✔️ Use RegisterAllInVogenEfCoreConverters() in ConfigureConventions
❌ Do NOT use HasVogenConversion() unless overriding behavior

This gives you the cleanest, most automatic setup, and you avoid cluttering every EntityTypeConfiguration with boilerplate.