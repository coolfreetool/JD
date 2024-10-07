# Simple JD CLI app

## Usage
~~~bash
# Load serialized JD model and show its params
dotnet run --framework net5.0 show path/to/model.jd

# Load and solve serialized JD model using SCIP solver
SOLVER=SCIP dotnet run --framework net5.0 solve path/to/model.jd

# Solve simple hardcoded example
SOLVER=SCIP dotnet run --framework net5.0 example1
~~~
