using CEAconsole.Models;
using CEAconsole.Services;
using CEAconsole.ViewModels;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using MathNet.Numerics.Providers.SparseSolver;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Symbolics;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;
using System.Xml.Linq;
using ScottPlot.Colormaps;
using System.Text.RegularExpressions;

namespace TestCEAconsole
{
    [TestClass]
    public class TestGaussianEliminationMethods
    {
        [TestMethod]
        public void TestAugmentedMatrixRowOperations()
        {
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                { 2.0, 1.0, -1.0, 5.0},
                { 4.0, -3.0, 2.0, 3.0},
                { 1.0, 2.0, 3.0, 10.0}
               });
            Assert.IsNotNull(matrix);

            Matrix<double> matrix2 = Matrix<double>.Build.DenseOfArray(new[,]{
                { 2.0, 1.0, -1.0},
                { 4.0, -3.0, 2.0},
                { 1.0, 2.0, 3.0}
               });

            var Row1 = matrix.Row(0);
            var Row2 = matrix.Row(1);
            var Row3 = matrix.Row(2);
            Row2 = Row2 - matrix.At(1, 0) / matrix.At(0, 0) * Row1;
            matrix.SetRow(1, Row2);
            Row3 = Row3 - matrix.At(2, 0) / matrix.At(0, 0) * Row1;
            matrix.SetRow(2, Row3);
            // diagonal
            var diagonal = matrix.Diagonal();
            // make pivot element
            Row2 = Row2 / -5;
            matrix.SetRow(1, Row2);
            diagonal = matrix.Diagonal();
            var lowerTriangleStrict = matrix.StrictlyLowerTriangle();
            Row3 = Row3 - 1.5 * Row2;
            matrix.SetRow(2, Row3);
            lowerTriangleStrict = matrix.StrictlyLowerTriangle();
            diagonal = matrix.Diagonal();

            double m_z = matrix.At(2, 3) / matrix.At(2, 2);
            double m_y = matrix.At(1, 3) + Math.Abs(matrix.At(1, 2)) * m_z;
            double m_x = (5 - (m_y - m_z)) / 2;
            Vector<double> result = Vector<double>.Build.Dense(3, 0.0);
            Vector<double> input = Vector<double>.Build.Dense([5.0, 3.0, 10.0]);
            var m_test = matrix2.Solve(input);

            Assert.IsNotNull(result);

        }
    }
    [TestClass]
    public class TestFilters
    {

        [TestMethod]
        public void ProdFilterShouldReturnOnlyList()
        {
            string json = InputCardService.GetInputCard();

            string[]? Prod = ProdFilter.ExtractProducts(json);
            string ar = Prod[0].ToString();

            Assert.AreEqual(20, Prod.Length);
            Assert.AreEqual("Ar", ar);
        }

        [TestMethod]
        public void TestICollection()
        {
            ICollection<Reactant> reactants = InputServices.GetSpecies("Data/newShortThermo.json");

            List<Reactant>? filteredCollection = reactants?.Where(item => item.Name == "CH4").ToList();
            var molecularWeight = (from item in filteredCollection
                                   select item.MolecularWeight).FirstOrDefault();
            double expected = 16.0424600;

            Dictionary<string, CEAconsole.Models.Temperature_Range>.ValueCollection? tempRange = (from item in filteredCollection
                                                                                                  select item.TemperatureRange.Values).FirstOrDefault();

            Dictionary<string, double>? chemFormula = (from item in filteredCollection
                                                       select item.Molecule.ChemicalFormula).FirstOrDefault();
            int? elementCount = chemFormula?.Count;
            elementCount ??= 0;
            string? symbol = chemFormula?.ElementAt(0).Key;
            symbol ??= string.Empty;
            double? atoms = chemFormula?.ElementAt(0).Value;
            atoms ??= 0;

            CEAconsole.Models.Temperature_Range? mx = tempRange?.ElementAt(0);

            Assert.AreEqual(expected, molecularWeight);
            //Assert.AreEqual(99, tempRange.ElementAt(1));

        }

        [TestMethod]
        public void TestShouldReturnListOfPropertiesForOneType()
        {
            ICollection<CPHSRef> CPHSDefaults = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            var defList = from item in CPHSDefaults.Where(r => r.Species_Name == "CH4") select item;

            double molecularWeight = (from item in CPHSDefaults.Where(r => r.Species_Name == "CH4")
                                      select item.Molecular_Weight).FirstOrDefault();

            var enthalpy = (from item in CPHSDefaults.Where(r => r.Species_Name == "CH4")
                            select item.Enthalpy).FirstOrDefault();

            Assert.AreEqual(16.04246, molecularWeight);
            Assert.AreEqual(-84.616, enthalpy);
            Assert.AreEqual(1, defList.Count<CPHSRef>());
        }

    }

    [TestClass]
    public class Test_Matrix_Methods
    {
        [TestMethod]
        public void TestPerplexity_Compressed_SparseColumn_Method()
        {
            // create a sparse matrix
            //double[,] xvalues = new double[,]
            //{
            //    {1, 0, 3 },
            //    {0, 2, 0},
            //    {4, 0, 5 }
            //};

            // Compressed Sparse Column format
            // column major
            double[] values = new double[] { 1, 3, 4, 2, 5 };
            int[] rowIndices = new int[] { 0, 0, 2, 1, 2 };
            int[] columnPointers = new int[] { 0, 2, 3, 5 };

            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseColumnFormat(3, 3, values.Length, rowIndices, columnPointers, values);
            // define the right hand side
            Vector<double> b = Vector.Build.Dense(new double[] { 6, 4, 10 });
            // solve the equation
            Vector<double> solution = A.Solve(b);   // 1.5, 0, 2
            Vector<double> expected = Vector.Build.Dense(new double[] { 1.5, 0.0, 2.0 });

            Assert.AreEqual(expected, solution);

            Assert.AreEqual(2, solution[2]);

        }
        [TestMethod]
        public void TestSparseMatrix_Compressed_Sparse_Row_Method()
        {
            double[] nonZeroValues = new double[] { 1, 3, 2, 4, 5 };
            // IA vector has a size of m + 1, where m is the number of rows in the matrix
            // it stores the cumulative number of non-zero elements up to ( but not including) the i-th row
            // IA[0] = 0, IA[i] = IA[i-1] + number of non-zero elements in the (i-1)-th row in the matrix
            int[] IA_rowPointers = new int[] { 0, 2, 3, 5 };
            int[] JA_columnIndices = new int[] { 0, 2, 1, 0, 2 }; // the column that holds value


            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseRowFormat(3, 3, nonZeroValues.Length, IA_rowPointers, JA_columnIndices, nonZeroValues);
            // define the right hand side
            Vector<double> b = Vector.Build.Dense(new double[] { 6, 4, 10 });
            // solve the problem
            Vector<double> solution = A.Solve(b);

            Assert.AreEqual(2, solution[1]);
        }

        [TestMethod]
        public void Test_Thermodynamic_BalanceHydrocarbonEquation_Method()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix_CH4 = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            Vector<double> expected_CH4 = Vector<double>.Build.Dense(new double[] { 1, 2, 1, 2 });
            var solution = ThermoDynamics.BalanceHydrocarbonEquation(matrix_CH4);
            Assert.AreEqual(expected_CH4, solution);



        }
        [TestMethod]
        public void TestShouldPutNonZeroValuesInto_ValuesVector()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            int numRows = matrix.RowCount;
            int nonZeroValues = 0;
            int ii = 0;

            IEnumerable<(int, Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        nonZeroValues++;
                    }
                }

            }

            double[] values = new double[nonZeroValues];
            //Vector<double> values = Vector<double>.Build.Dense(nonZeroValues);
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        values[ii] = item;
                        ii++;
                        //nonZeroValues++;
                    }
                }

            }
            Assert.AreEqual(8, values.Length);

        }
        [TestMethod]
        public void TestGetNumberOfNonZeroValuesInMatrix()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            int numRows = matrix.RowCount;
            int nonZeroValues = 0;

            IEnumerable<(int, Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;

                foreach (var item in m_rowVector)
                {
                    int m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        nonZeroValues++;
                    }
                }
            }

            Assert.AreEqual(8, nonZeroValues);

        }
        [TestMethod]
        public void Test_Should_ProperlySize_IA_JA_vectors()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });
            Assert.IsNotNull(matrix);

            int numRows = matrix.RowCount;
            int numColumns = matrix.ColumnCount;
            Assert.AreEqual((int)numRows, matrix.RowCount);

            // Create IA vector = numrows + 1
            Vector<double> IA = Vector.Build.Dense(numRows + 1);
            Assert.AreEqual(IA.Count, matrix.RowCount + 1);

            // Create the JA vector = number of reactants + number of products
            // this is set manully here but will get count from input. TODO
            Vector<double> JA = Vector.Build.Dense(8);
            Assert.AreEqual(8, JA.Count);

            IEnumerable<(int, Vector<double>)> m_enumeratedRows = matrix.EnumerateRowsIndexed();
            IEnumerable<(int, Vector<double>)> m_enumeratedColumns = matrix.EnumerateColumnsIndexed();

            int totalNonZeroValues = 0;
            //
            Vector<double> expected_IA = Vector.Build.Dense(new double[] { 0, 2, 4, 7, 8 });
            Vector<double> expected_JA = Vector.Build.Dense(new double[] { 0, 2, 0, 3, 1, 2, 3, 0 });
            int jj = 0;

            for (int i = 0; i < numRows; i++)
            {
                var m_elementAtRow = m_enumeratedRows.ElementAt(i);
                int m_rowNumber = m_elementAtRow.Item1;
                Vector<double> m_rowVector = m_elementAtRow.Item2;
                int Row_non_zero_values = 0;
                int column_where_value_found = 0;

                foreach (var item in m_rowVector)
                {
                    var m_compare = item.CompareTo(0.0);
                    if (m_compare != 0)
                    {
                        // increment valuesCount
                        Row_non_zero_values++;
                        // what column was it found in
                        // { 0, 2, 0, 3, 1, 2, 3, 0 };
                        //int m_c = column_where_value_found;

                        JA[jj] = column_where_value_found;
                        jj++;

                    }
                    column_where_value_found++;

                }
                IA[i + 1] = IA[i] + Row_non_zero_values;
                totalNonZeroValues += Row_non_zero_values;

            }
            Assert.AreEqual(8, totalNonZeroValues);
            Assert.IsNotNull(m_enumeratedRows);
            Assert.AreEqual(expected_IA, IA);
            Assert.AreEqual(expected_JA, JA);

        }

        [TestMethod]
        public void Test_Should_Balance_Equation()
        {
            //    { 1.0, 0.0,  -1.0,  0.0 }, // C balance
            //    { 4.0, 0.0,   0.0, -2.0 }, // H balance
            //    { 0.0, 2.0,  -2.0, -1.0 }, // O balance
            //    { 1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            int rows = 12;
            int columns = 12;
            var spm = new SparseMatrix(rows, columns);
            Assert.IsNotNull(spm);

            // CH4          O2                              CO2                 H2O
            // column 0     column 1        column 2        column 3            column 4
            // empty row
            spm[1, 0] = 1.0; spm[1, 3] = -1.0;                       // Carbon
            spm[2, 0] = 4.0; spm[2, 4] = -2.0;   // Hydrogen
            spm[3, 1] = 2.0; spm[3, 3] = -2.0; spm[3, 4] = -1.0;   // Oxygen
            //*/// empty row
            spm[5, 0] = 1.0;
            double[] values = new double[] { 1.0, -1.0, 4.0, -2.0, 2.0, -2.0, -1.0, 1.0 };

            int[] IA = new int[] { 0, 2, 4, 7, 8 };
            int[] IJ = new int[] { 0, 2, 0, 3, 1, 2, 3, 0 };
            Matrix<double> A = Matrix.Build.SparseFromCompressedSparseRowFormat(4, 4, values.Length, IA, IJ, values);
            // define the right hand side
            Vector<double> b = Vector.Build.Dense(new double[] { 0, 0, 0, 1.0 });
            // expected
            Vector<double> expected = Vector<double>.Build.Dense(new double[] { 1, 2, 1, 2 });

            // solution
            Vector<double> solution = A.Solve(b);

            Assert.AreEqual(expected, solution);

        }
        [TestMethod]
        public void TestMathNetMatrix()
        {
            // CH4 + O2 =  CO2 + H2O
            // CH4 + 2O2 = CO2 + 2H2O
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(new[,]{
                {1.0, 0.0,  -1.0,  0.0 }, // C balance
                {4.0, 0.0,   0.0, -2.0 }, // H balance
                {0.0, 2.0,  -2.0, -1.0 }, // O balance
                {1.0, 0.0,   0.0,  0.0 }   // Setting CH4
            });

            var spm = SparseMatrix.Create(6, 6, 0.0);

            // empty row
            spm[1, 0] = 1.0; /*spm[1, 1] = 0.0;*/              spm[1, 3] = -1.0; /*spm[1, 4] =  0.0;*/
            spm[2, 0] = 4.0; /*spm[2, 1] = 0.0;*/              /*spm[2, 3] =  0.0;*/ spm[2, 4] = -2.0;
            /*spm[3, 0] = 0.0;*/
            spm[3, 1] = 2.0; spm[3, 3] = -2.0; spm[3, 4] = -1.0;
            //*/// empty row
            spm[5, 0] = 1.0; /*spm[5, 1] = 0.0;                  spm[5, 3] = 0.0;  spm[5, 4] = 0.0;*/ // set compound counts
                                                                                                      // empty column

            int non_zero = spm.NonZerosCount;
            Vector<double> rowAbsSums = spm.RowAbsoluteSums();
            Vector<double> rowSums = spm.RowSums();
            Vector<double> columnAbsSums = spm.ColumnAbsoluteSums();
            Vector<double> columnSums = spm.ColumnSums();
            double[] columnMajor = spm.ToColumnMajorArray();
            double[] rowMajor = spm.ToRowMajorArray();
            Matrix<double> lowerTri = spm.LowerTriangle();
            Matrix<double> lowerTriStrict = spm.StrictlyLowerTriangle();
            Matrix<double> upperTri = spm.UpperTriangle();
            Matrix<double> upperTriStrict = spm.StrictlyUpperTriangle();
            var transMul = lowerTriStrict.TransposeAndMultiply(lowerTri);
            // set values of matrix
            //matrix[0, 0] = 1; matrix[0, 1] = 0; matrix[0, 2] = -1; matrix[0, 3] = 0;
            //matrix[1, 0] = 4; matrix[1, 1] = 0; matrix[1, 2] = 0;  matrix[1, 3] = -2;
            //matrix[2, 0] = 0; matrix[2, 1] = 2; matrix[2, 2] = -2; matrix[2, 3] = -1;
            // count matrix columns should equal 4
            int columnCount = matrix.ColumnCount;

            // create right hand side vector
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });

            // number of columns
            var spmSparceVector = SparseVector.Create(6, 0.0);
            spmSparceVector[5] = 1.0;
            // solve the system using Gaussian elimination
            Vector<double> solution = matrix.Solve(rightHandside); // 1,2,1,2
            Vector<double> m_result = spm.Solve(spmSparceVector);

            Assert.AreEqual(4, columnCount);
            Assert.AreEqual(4, solution.Count);
            Assert.AreEqual(1, solution[0]);
            Assert.AreEqual(2, solution[1]);
            Assert.AreEqual(1, solution[2]);
            Assert.AreEqual(2, solution[3]);

        }
        [TestMethod]
        public void TestBalancedEquationSolverWithReactantInput()
        {
            // Arrange
            ICollection<Reactant> reactants = InputServices.GetSpecies("Data/newShortThermo.json");
            // reactants
            string fuelName = "CH4";
            string oxidizerName = "O2";
            List<Reactant>? Fuel = reactants?.Where(item => item.Name == fuelName).ToList();
            List<Reactant>? Oxidizer = reactants?.Where(item => item.Name == oxidizerName).ToList();
            // products
            string CO2Name = "CO2";
            string H2OName = "H2O";
            List<Reactant>? CO2 = reactants?.Where(item => item.Name == CO2Name).ToList();
            List<Reactant>? H2O = reactants?.Where(item => item.Name == H2OName).ToList();

            Dictionary<string, double>? fuelElementsList = (from molecule in Fuel
                                                            select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            Dictionary<string, double>? oxidizerElementsList = (from molecule in Oxidizer
                                                                select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            Dictionary<string, double>? co2ElementsList = (from molecule in CO2
                                                           select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            Dictionary<string, double>? h2oElementsList = (from molecule in H2O
                                                           select molecule.Molecule.ChemicalFormula).FirstOrDefault();

            // Reactants
            //Fuel
            fuelElementsList.TryGetValue("C", out double reactant_fuel_carbon_value);
            fuelElementsList.TryGetValue("H", out double reactant_fuel_hydrogen_value);
            fuelElementsList.TryGetValue("O", out double reactant_fuel_oxygen_value);
            // Oxidizer
            oxidizerElementsList.TryGetValue("C", out double reactant_oxidizer_carbon_value);
            oxidizerElementsList.TryGetValue("H", out double reactant_oxidizer_hydrogen_value);
            oxidizerElementsList.TryGetValue("O", out double reactant_oxidizer_oxygen_value);

            // Products
            //CO2
            co2ElementsList.TryGetValue("C", out double product_CO2_carbon_value);
            co2ElementsList.TryGetValue("H", out double product_CO2_hydrogen_value);
            co2ElementsList.TryGetValue("O", out double product_CO2_oxygen_value);
            // H2O
            h2oElementsList.TryGetValue("C", out double product_H2O_carbon_value);
            h2oElementsList.TryGetValue("H", out double product_H2O_hydrogen_value);
            h2oElementsList.TryGetValue("O", out double product_H2O_oxygen_value);

            double[,] matrixValues =
            {
                {reactant_fuel_carbon_value, reactant_oxidizer_carbon_value, - product_CO2_carbon_value, - product_H2O_carbon_value},
                {reactant_fuel_hydrogen_value, reactant_oxidizer_hydrogen_value ,  - product_CO2_hydrogen_value, - product_H2O_hydrogen_value},
                {reactant_fuel_oxygen_value,reactant_oxidizer_oxygen_value, - product_CO2_oxygen_value,  - product_H2O_oxygen_value },
                {1.0, 0.0, 0.0, 0.0 }
            };
            Matrix<double> matrix = Matrix<double>.Build.DenseOfArray(matrixValues);
            // create right hand side vector
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });
            // solve the system using Gaussian elimination
            Vector<double> solution = matrix.Solve(rightHandside);

            Matrix<double> defaultMatrix = Matrix<double>.Build.Dense(4, 4, 0.0);
            defaultMatrix[0, 0] = 1.0; defaultMatrix[0, 1] = 0.0; defaultMatrix[0, 2] = -1.0; defaultMatrix[0, 3] = 0.0;
            defaultMatrix[1, 0] = 4.0; defaultMatrix[1, 1] = 0.0; defaultMatrix[1, 2] = 0.0; defaultMatrix[1, 3] = -2.0;
            defaultMatrix[2, 0] = 0.0; defaultMatrix[2, 1] = 2.0; defaultMatrix[2, 2] = -2.0; defaultMatrix[2, 3] = -1.0;
            defaultMatrix[3, 0] = 1.0; defaultMatrix[3, 1] = 0.0; defaultMatrix[3, 2] = 0.0; defaultMatrix[3, 3] = 0.0;

            Vector<double> rightside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 1.0 });
            // solve the system using Gaussian elimination
            Vector<double> m_solution = matrix.Solve(rightside);

            List<Reactant> balancedProductH2O = H2O;
            balancedProductH2O.ForEach(r => r.Molecule.Count = m_solution[3]);

            Molecule balancedFuel = new()
            {
                ChemicalFormula = new()
                {
                    {"C", 1.0 },
                    {"H", 4.0 }
                },
                Count = m_solution[0]
            };
            Molecule balancedO2 = new()
            {
                ChemicalFormula = new()
                {
                    { "O", 2.0 },
                },
                Count = m_solution[1]
            };
            Molecule balancedCO2 = new()
            {
                ChemicalFormula = new()
                {
                    {"C", 1.0 },
                    {"O", 2.0 }
                },
                Count = m_solution[2]
            };
            Molecule balancedH2O = new()
            {
                ChemicalFormula = new()
                {
                    { "H", 2.0 },
                    { "O", 1.0 }
                },
                Count = m_solution[3]
            };

            Assert.AreEqual(99, 0);

        }
    }

    [TestClass]
    public class TestRegex
    {
        [TestMethod]
        public void TestShouldReturnMoleculeCountAndFormula()
        {
            // Arrange
            string molecule = "CH4";
            string formula = "4C12ClO3";
            // TODO Update to NASApolynomials
            ICollection<Reactant> reactants = InputServices.GetSpecies("Data/newShortThermo.json");
            ICollection<Species> species = InputServices.GetNASA("Data/NASApolynomials.json");
            Assert.IsNotNull(reactants);
            var m_Molecule = from item in reactants
                             where item.Name == molecule
                             select item.Molecule;

            var expected = m_Molecule;

            var m_CH4 = from item in species
                        where item.Name == molecule
                        select item;

            var msplit = SplitMolecule(formula);

            string symbol = "";
            double numberAtoms = 0.0;
            foreach (var item in m_CH4.ElementAt(0).ChemicalFormula)
            {
                symbol = item.Symbol;
                numberAtoms = item.NumberOfAtoms;
            }


            // Act
            //var result = MoleculeOperations.SplitMolecule(molecule);

            // Assert
            Assert.IsNotNull(m_Molecule);

        }

        private static string SplitMolecule(string formula)
        {
            // matches elements and numbers
            string pattern = @"(\d+)?([A-Z][a-z]?)(\d*)";
            // find all matches in the molecule string
            MatchCollection matchCollection = Regex.Matches(formula, pattern);
            // List to hold the split parts
            List<string> splitParts = new();
            foreach (Match match in matchCollection)
            {
                // capture the element symbol
                string coefficient = match.Groups[1].Value;
                string element = match.Groups[2].Value;
                string quantity = match.Groups[3].Value;
                // combine the element and quantity with a space (if quantity exists)
                //splitParts.Add(element + (string.IsNullOrEmpty(quantity) ? "" : " " + quantity));
                splitParts.Add((string.IsNullOrEmpty(coefficient) ? "" : coefficient + " ") + element + (string.IsNullOrEmpty(quantity) ? "" : " " + quantity));
                //join the parts with 2 spaces as per the requirement
                
            }
            return string.Join(" ", splitParts);
        }
    }

    [TestClass]
    public class Test_NASA_Polynomials
    {
        [TestMethod]
        public void Test_NASAnotNull()
        {
            ICollection<Species> species = InputServices.GetNASA("Data/NASApolynomials.json");
            Assert.IsNotNull(species);
        }

        [TestMethod]
        public void TestShouldReturnNamedSpecie()
        {
            string name = "CH4";
            ICollection<Species> species = InputServices.GetNASA("Data/NASApolynomials.json");
            var m_specie = from item in species
                           where item.Name == name
                           select item;
            var expected = m_specie;

            Assert.IsNotNull(m_specie);

        }

        [TestMethod]
        public void TestShouldReturnPhasesGreaterThanZero()
        {
            ICollection<Species> species = InputServices.GetNASA("Data/NASApolynomials.json");
            var m_specie = from item in species
                           where item.PhaseValue > 0
                           select item;
            Assert.AreNotEqual(0, m_specie.Count());

        }

    }

    [TestClass]
    public class MatrixSolvers
    {
        [TestMethod]
        public void TestShouldBuildMatrixAndLoadChemicalFormula()
        {
            // Arrange
            Matrix<double> coefficientMatrix = Matrix<double>.Build.Dense(11, 11, 0.0);
            double determinate = coefficientMatrix.Determinant();
            bool isSymetric = coefficientMatrix.IsSymmetric();
            Assert.IsNotNull(coefficientMatrix);
            //

            Matrix<double> variableMatrix = Matrix<double>.Build.DenseIdentity(11, 11);
            Assert.IsNotNull(variableMatrix);

            // constants matrix
            Matrix<double> constantMatrix = new DenseMatrix(11, 1);
            constantMatrix[0, 0] = 0;
            constantMatrix[1, 0] = 0;
            constantMatrix[2, 0] = 0;
            constantMatrix[3, 0] = 0;
            constantMatrix[4, 0] = 0;
            constantMatrix[5, 0] = 0;
            constantMatrix[6, 0] = 0;
            constantMatrix[7, 0] = 0;
            constantMatrix[8, 0] = 0;
            constantMatrix[9, 0] = 0;
            constantMatrix[10, 0] = 1.0;
            Assert.IsNotNull(constantMatrix);

            // create rightHand side vector
            int m_VectorSize = 11;
            //Vector<double> rightHandSide = Vector<double>.Build.Dense(m_VectorSize, 0.0);
            Vector<double> rightHandside = Vector<double>.Build.Dense(new[]
            {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0 });
            int m_rightHandSideCount = rightHandside.Count;
            rightHandside[m_rightHandSideCount - 1] = 1.0;
            Assert.IsNotNull(rightHandside);
            Assert.AreEqual(m_VectorSize, m_rightHandSideCount);

            //
            // Set the first column (column 0) to the values for the first reactant(Species)
            // Set the number of CH4 molecules to 1 in the last row of the defaultMatrix
            coefficientMatrix[10, 0] = 1.0;
            coefficientMatrix[10, 1] = 1.0;
            coefficientMatrix[10, 2] = 1.0;
            coefficientMatrix[10, 3] = 1.0;
            coefficientMatrix[10, 4] = 1.0;
            coefficientMatrix[10, 5] = 1.0;
            coefficientMatrix[10, 6] = 1.0;
            coefficientMatrix[10, 7] = 1.0;
            coefficientMatrix[10, 8] = 1.0;
            coefficientMatrix[10, 9] = 1.0;
            coefficientMatrix[10, 10] = 1.0;

            // Get data for Species, TableOfElements, And Reference Values
            // Table of elements
            ICollection<Element> tableOfElements = ElementsService.GetElements("Data/tableOfElements.json");
            Assert.IsNotNull(tableOfElements);
            // Get the reference data
            ICollection<CPHSRef> cphs_reference = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            Assert.IsNotNull(cphs_reference);
            // Species data 
            // TODO Update to NASApolynomials
            ICollection<Reactant> AllSpecies = InputServices.GetSpecies("Data/newShortThermo.json");
            Assert.IsNotNull(AllSpecies);

            // Reactants and products section Section
            string m_firstReactantMolecule = "CH4";
            string m_secondReactantMolecule = "O2";

            // get the key collection of elements in reactants
            var inputKeyCollection = from item in AllSpecies
                                     where item.Name == m_firstReactantMolecule | item.Name == m_secondReactantMolecule
                                     select item.Molecule.ChemicalFormula?.Keys;

            Dictionary<string, Element?> elementTableOfElements = new();
            for (int i = 0; i < inputKeyCollection.Count(); i++)
            {
                var elementAt = inputKeyCollection.ElementAt(i);
                foreach (var item in elementAt)
                {
                    IEnumerable<Element> elementData = tableOfElements.Where(x => x.Symbol == item);
                    string key = item;
                    if (elementData.Any())
                    {
                        elementTableOfElements.Add(key: key, value: elementData.FirstOrDefault());
                    };
                }
            }

            //IEnumerable<KeyValuePair<string, Element>> elementProperties = from item in elementTableOfElements
            //                   where item.Key == "C"
            //                   select item;
            //IEnumerable<CPHSRef> element_cphs_reference_defaults = from item in cphs_reference
            //          where item.Species_Name == "C"
            //          select item;

            for (int i = 0; i < elementTableOfElements.Count; i++)
            {
                string key = elementTableOfElements.ElementAt(i).Key;
                IEnumerable<KeyValuePair<string, Element>> elementProperties = from item in elementTableOfElements
                                                                               where item.Key == key
                                                                               select item;

                IEnumerable<CPHSRef> element_cphs_reference_defaults = from item in cphs_reference
                                                                       where item.Species_Name == key
                                                                       select item;
            }
            string mx_key = "C";
            var keyValuePairsElements = elementTableOfElements.FirstOrDefault(x => x.Key == mx_key);

            IEnumerable<Molecule> m_firstReactant = from item in AllSpecies
                                                    where item.Name == m_firstReactantMolecule
                                                    select item.Molecule;
            Assert.IsNotNull(m_firstReactant);


            Dictionary<string, double>.KeyCollection keyCollection = m_firstReactant.FirstOrDefault().ChemicalFormula.Keys;
            Dictionary<string, double>.ValueCollection valuesCollection = m_firstReactant.FirstOrDefault().ChemicalFormula?.Values;
            int m_keyCount = keyCollection.Count;
            IEnumerable<Molecule> m_secondReactant = from item in AllSpecies
                                                     where item.Name == m_secondReactantMolecule
                                                     select item.Molecule;
            Assert.IsNotNull(m_secondReactant);
            Collection<IEnumerable<Molecule>> reactantsCollection = new();
            reactantsCollection.Add(m_firstReactant);
            reactantsCollection.Add(m_secondReactant);

            // Products Section
            // first product = "CO2
            // second product = H2O
            string m_firstProductMolecule = "CO2";
            string m_secondProductMolecule = "H2O";

            var m_firstproduct = from item in AllSpecies
                                 where item.Name == m_firstProductMolecule
                                 select item.Molecule;
            Assert.IsNotNull(m_firstproduct);
            var m_secondProduct = from item in AllSpecies
                                  where item.Name == m_secondProductMolecule
                                  select item.Molecule;
            Assert.IsNotNull(m_secondProduct);
            Collection<IEnumerable<Molecule>> productsCollection = new();
            productsCollection.Add(m_firstproduct);
            productsCollection.Add(m_secondProduct);

            int matrixColumnCount = 0;
            int m_reactantCount = reactantsCollection.Count;
            foreach (var item in reactantsCollection)
            {
                Dictionary<string, double>.KeyCollection c_keys = item.FirstOrDefault().ChemicalFormula.Keys;
                Dictionary<string, double>.ValueCollection c_values = item.FirstOrDefault().ChemicalFormula.Values;
                int c_keycount = c_keys.Count;

                LoadBalancedEquationMatrix(c_keys, c_values, c_keycount, matrixColumnCount);
                matrixColumnCount = matrixColumnCount + 1;
            }

            // since these are the products the values have to be set to negative
            foreach (var item in productsCollection)
            {
                //Dictionary<string, double>.KeyCollection c_keys = item.FirstOrDefault().ChemicalFormula.Keys;
                Dictionary<string, double>? keyValuePairs = item.FirstOrDefault().ChemicalFormula;
                //Dictionary<string, double>.ValueCollection c_values = item.FirstOrDefault().ChemicalFormula.Values;
                foreach (var kvp in keyValuePairs)
                {
                    keyValuePairs[kvp.Key] = -kvp.Value;
                }
                //int c_keycount = c_keys.Count;

                LoadBalancedEquationMatrix(keyValuePairs.Keys, keyValuePairs.Values, keyValuePairs.Count, matrixColumnCount);
                matrixColumnCount = matrixColumnCount + 1;
            }

            // TODO j is temp variable to reference column in defaultMatrix

            //(Matrix<double> solutionMatrix, Matrix<double> pivotMatrix) = coefficientMatrix.Solve(input: variableMatrix, constantMatrix);

            void LoadBalancedEquationMatrix(Dictionary<string, double>.KeyCollection Keys, Dictionary<string, double>.ValueCollection Values, int KeysCount, int ColumnNumber)
            {
                int m_column = ColumnNumber;

                for (int i = 0; i < KeysCount; i++)
                {
                    string elementKey = Keys.ElementAt(i);
                    switch (elementKey)
                    {
                        case "H":
                            // matrix row 0
                            int row0 = 0;
                            coefficientMatrix[row0, m_column] = Values.ElementAt(i);
                            break;
                        case "D":
                            // matrix row 1
                            int row1 = 1;
                            coefficientMatrix[row1, m_column] = Values.ElementAt(i);
                            break;
                        case "He":
                            // matrix row 2
                            int row2 = 2;
                            coefficientMatrix[row2, m_column] = Values.ElementAt(i);
                            break;
                        case "Li":
                            // matrix row 3
                            int row3 = 3;
                            coefficientMatrix[row3, m_column] = Values.ElementAt(i);
                            break;
                        case "Be":
                            // matrix row 4
                            int row4 = 4;
                            coefficientMatrix[row4, m_column] = Values.ElementAt(i);
                            break;
                        case "B":
                            // matrix row 5
                            int row5 = 5;
                            coefficientMatrix[row5, m_column] = Values.ElementAt(i);
                            break;
                        case "C":
                            // matrix row 6
                            int row6 = 6;
                            coefficientMatrix[row6, m_column] = Values.ElementAt(i);
                            break;
                        case "N":
                            // matrix row 7
                            int row7 = 7;
                            coefficientMatrix[row7, m_column] = Values.ElementAt(i);
                            break;
                        case "O":
                            // matrix row 8 
                            int row8 = 8;
                            coefficientMatrix[row8, m_column] = Values.ElementAt(i);
                            break;
                        case "F":
                            // matrix row 9
                            int row9 = 9;
                            coefficientMatrix[row9, m_column] = Values.ElementAt(i);
                            break;

                        default:
                            break;
                    }
                }
            }

            // solve the system using Gaussian elimination
            Vector<double> m_solution = coefficientMatrix.Solve(rightHandside);

            Assert.AreEqual(99, 0);
        }
    }

    [TestClass]
    public class TestServices
    {

        [TestMethod]
        public void Test_ElementsService()
        {
            ICollection<Element> json = InputServices.GetTableOfElements("Data/TableOfElements.json");
            int elementCount = json.Count;

            Assert.AreNotEqual(0, elementCount);
        }

        [TestMethod]
        public void TestInputServicesWithPath()
        {
            ICollection<Reactant> json = InputServices.GetSpecies("Data/thermoInp.json");
            int reactantCount = json.Count;

            Assert.AreEqual(2000, reactantCount);
        }

        [TestMethod]
        public void Test_Ref_DefaultServiceWithPath()
        {
            ICollection<CPHSRef> ref_defaults = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            int ref_defaultsCount = ref_defaults.Count;

            Assert.AreEqual(1258, ref_defaultsCount);
        }

        [TestMethod]
        public void Test_ShouldReturnReferenceElements()
        {
            ICollection<ReferenceElements> referenceElements = InputServices.GetReferenceElements("Data/refElements.json");
            int referenceElementsCount = referenceElements.Count;
            Assert.AreEqual(referenceElementsCount, referenceElements.Count);
        }

    }

    [TestClass]
    public class TestReferenceElements
    {
        [TestMethod]
        public void Test_ReferenceElementHeatCapacity()
        {
             // Update with the refElements.json file
            // Testing for Oxygen = O
     //               "temperatureRange": [ 200.000, 1000.000 ],
					//"numberOfCoefficients": 7,
					//"tExponents": [ -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0 ],
					//"hJmol": 6725.403,
					//"coefficients": [ -7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13 ],
					//"integrationConstants": [ 2.840362437e+04, 8.404241820e+00 ]
            List<double> temperatureRange = [200.0, 1000.0];
            List<double> t_exp = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coeff = [-7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13];
            List<double> integrationConstants = [2.840362437e+04, 8.404241820e+00];

            double delta = 0.005;
            double T = 398.15;
            double Cp = ThermoDynamics.HeatCapacity(T, coeff, t_exp);

            Assert.AreEqual(21.488, Cp, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 249.175)]
        [DataRow(398.15, 251.343)]
        [DataRow(498.15, 253.479)]
        [DataRow(598.15, 255.598)]
        [DataRow(698.15, 257.706)]
        [DataRow(798.15, 259.807)]
        [DataRow(898.15, 261.903)]
        [DataRow(998.15, 263.996)]
        [DataRow(1000.00, 264.035)]
        public void TestShouldReturnEnthaplyForElementOxygen(double Temperature, double expected)
        {
            string searchString = "O";
            ICollection<Species> m_species = InputServices.GetNASA("Data/NASApolynomials.json");

            IEnumerable<Species> m_reactant = from specie in m_species
                                              where specie.Name == searchString
                                              select specie;

            ICollection<ChemicalFormula> chemicalFormula = m_reactant.First().ChemicalFormula;
            List<double> temperatureRange = m_reactant.First().DataRecords.ElementAt(0).TemperatureRange;
            List<double> coefficients = m_reactant.First().DataRecords.ElementAt(0).Coefficients;
            List<double> t_expnts = m_reactant.First().DataRecords.ElementAt(0).TExponents;
            List<double> integrationConstants = m_reactant.First().DataRecords.ElementAt(0).IntegrationConstants;
            double delta = 0.005;
            double refTemp = 298.15;
            double heatOfFormation = m_reactant.First().HeatOfFormation;
            double Enthalpy_kJmol = ThermoDynamics.Enthalpy(refTemp, heatOfFormation, Temperature, coefficients, t_expnts);

            Assert.AreEqual(expected, Enthalpy_kJmol, delta);
        }

        [TestMethod]
        public void TestShouldAddCarbonAndHydrogenEnthalpy()
        {
             // TODO update with NASApolynomials
            // CH4 
            // CH4 coefficients
            List<double> CH_t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> CH_coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> CH_integrationConstants = [-2.331314360e+04, 8.904322750e+01];
            double CH4HeatOfFormation = -74600.0;
            double chHeat = -74.600;

            double ref_temp = 298.15;
            double T = 298.15;
            // get Carbon enthalpy at reference temp
            double C_heatOfFormation = 716680.000;
            double C_hjmol = 6535.895;
            List<double> C_tExponents = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> C_coefficients = [6.495031470e+02, -9.649010860e-01, 2.504675479e+00, -1.281448025e-05, 1.980133654e-08, -1.606144025e-11, 5.314483411e-15];
			List<double> C_integrationConstants = [8.545763110e+04, 4.747924288e+00];
            // get H2
            double H2_heatOfFormation = 0.0;
            List<double> H2_tExponents = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> H2_coefficients = [4.078323210e+04, -8.009186040e+02, 8.214702010e+00, -1.269714457e-02, 1.753605076e-05, -1.202860270e-08, 3.368093490e-12];
			List< double > H2_integrationConstants = [2.682484665e+03, -3.043788844e+01];
            // get Hydrogen
            double H_heatOfFormation = 217998.828;
            double H_hjmol = 6197.428;
            List<double> H_tExponents = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> H_coefficients = [0.000000000e+00, 0.000000000e+00, 2.500000000e+00, 0.000000000e+00, 0.000000000e+00, 0.000000000e+00, 0.000000000e+00];
			List<double> H_integrationConstants = [ 2.547370801e+04, -4.466828530e-01 ];

            double C_enthalpy = ThermoDynamics.Enthalpy(ref_temp, C_heatOfFormation, T, C_coefficients, C_tExponents);
            double Cref = ThermoDynamics.EnthalpyRefH298(ref_temp,T,C_coefficients, C_tExponents);
            double H_enthalpy = ThermoDynamics.Enthalpy(ref_temp, H_heatOfFormation, T, H_coefficients, H_tExponents);
            double CH_enthalpy = ThermoDynamics.Enthalpy(ref_temp, CH4HeatOfFormation, T, CH_coefficients, CH_t_expnts);
            double H2_enthalpy = ThermoDynamics.Enthalpy(ref_temp, H2_heatOfFormation, T, H2_coefficients, H2_tExponents);
            double H2_ref = ThermoDynamics.EnthalpyRefH298(ref_temp, T, H2_coefficients, H2_tExponents);

            double m_sum = C_enthalpy - H_enthalpy * 4;

            Assert.AreEqual(99, m_sum);

        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 2.168)]
        [DataRow(498.15, 4.304)]
        [DataRow(598.15, 6.423)]
        [DataRow(698.15, 8.531)]
        [DataRow(798.15, 10.632)]
        [DataRow(898.15, 12.728)]
        [DataRow(998.15, 14.821)]
        [DataRow(1000.00, 14.860)]
        public void Test_ReferenceElementEnthalpy(double T, double expected)
        {
            // TODO update with refElements.json
            // O coefficients
            List<double> temperatureRange = [200.0, 1000.0];
            List<double> t_exp = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coeff = [-7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13];
            List<double> integrationConstants = [2.840362437e+04, 8.404241820e+00];

            double delta = 0.005;
            double ref_temp = 298.15;
            //double T = 398.15;

            double enthalpy = ThermoDynamics.EnthalpyRefH298(ref_temp, T, coeff, t_exp);
            Assert.AreEqual(expected, enthalpy, delta);

        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 2.168)]
        [DataRow(498.15, 4.304)]
        [DataRow(598.15, 6.423)]
        [DataRow(698.15, 8.531)]
        [DataRow(798.15, 10.632)]
        [DataRow(898.15, 12.728)]
        [DataRow(998.15, 14.821)]
        [DataRow(1000.00, 14.860)]
        public void Test_ReferenceElementH298(double T, double expected)
        {
            // O coefficients
            List<double> temperatureRange = [200.0, 1000.0];
            List<double> t_exp = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0,0.0];
            List<double> coeff = [-7.953611300e+03, 1.607177787e+02, 1.966226438e+00, 1.013670310e-03, -1.110415423e-06, 6.517507500e-10, -1.584779251e-13];
            List<double> integrationConstants = [2.840362437e+04, 8.404241820e+00];

            double delta = 0.005;
            double ref_temp = 298.15;

            double enthalpy = ThermoDynamics.EnthalpyRefH298(ref_temp, T, coeff, t_exp);

            Assert.AreEqual(expected, enthalpy, delta);
        }
    }

    [TestClass]
    public class TestThermodynamicMethods
    {
        private static readonly ICollection<CPHSRef> referenceCPHS = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
        private static readonly ICollection<Species> refElements = InputServices.GetNASA("Data/refElements.json");
        private static readonly ICollection<Species> species = InputServices.GetNASA("Data/NASApolynomials.json");
        private static readonly double referenceTemp = 298.15;
        private static readonly double delta = 0.005;
        private static readonly string searchString = "CH4";
        private static readonly IEnumerable<Species> m_reactant = from specie in species
                                          where specie.Name == searchString
                                          select specie;
        private static ICollection<ChemicalFormula> chemicalFormula = m_reactant.First().ChemicalFormula;
        private static readonly List<double> temperatureRange = m_reactant.First().DataRecords.ElementAt(0).TemperatureRange;
        private static List<double> coefficients = m_reactant.First().DataRecords.ElementAt(0).Coefficients;
        private static List<double> integrationConstants = m_reactant.First().DataRecords.ElementAt(0).IntegrationConstants;
        private static readonly List<double> t_expnts = m_reactant.First().DataRecords.ElementAt(0).TExponents;

        private static readonly IEnumerable<CPHSRef> m_referenceSpecie = (IEnumerable<CPHSRef>)(from m_specie in referenceCPHS
                                                                        where m_specie.Species_Name == searchString
                                                                        select m_specie);

        [TestMethod]
        public void TestEnthalpyOfReaction()
        {
            // H2(g) + CL2(g) <--> 2HCL(g)


            var reactant_1 = from item in referenceCPHS
                             where item.Species_Name == "H2"
                             select (item.Delta_Enthalpy_Ref, item.Entropy_Ref);

            var reactant_2 = from item in referenceCPHS
                             where item.Species_Name == "CL2"
                             select (item.Delta_Enthalpy_Ref, item.Entropy_Ref);

            var product_1 = from item in referenceCPHS
                            where item.Species_Name == "HCL"
                            select (item.Delta_Enthalpy_Ref, item.Entropy_Ref);

            var queryResult = from item in product_1
                              select (DeltaEnthalpyRef: item.Delta_Enthalpy_Ref, EntropyRef: item.Entropy_Ref);

            double productDeltaEnthalpyRef = 0.0;
            double productEntropyRef = 0.0;
            foreach ((double Delta_Enthalpy_Ref, double Entropy_Ref) in product_1)
            {
                productDeltaEnthalpyRef = Delta_Enthalpy_Ref;
                productEntropyRef = Entropy_Ref;
            }

            double reactant_1DeltaEnthalpyRef = 0.0;
            double reactant_1EntropyRef = 0.0;
            foreach (var item in reactant_1)
            {
                reactant_1DeltaEnthalpyRef = item.Delta_Enthalpy_Ref;
                reactant_1EntropyRef = item.Entropy_Ref;
            }
            double reactant_2DeltaEnthalpyRef = 0.0;
            double reactant_2EntropyRef = 0.0;
            foreach (var item in reactant_2)
            {
                reactant_2DeltaEnthalpyRef = item.Delta_Enthalpy_Ref;
                reactant_2EntropyRef = item.Entropy_Ref;
            }

            // delta H_0_reaction = SUM n_p * deltaHf_0(p) - SUM n_r deltaHf_0(r)
            var delta_H_0reaction = 2 * productDeltaEnthalpyRef - 1 * (reactant_1DeltaEnthalpyRef + reactant_2DeltaEnthalpyRef);
            double expected_deltaH = -184.62;
            Assert.AreEqual(expected_deltaH, delta_H_0reaction, delta);
            // Entropy of reaction
            // delta_S_0reaction = SUM n_p * S_0(p) - SUM n(r) * S_0(r)


            double delta_S_0reaction = (2 * productEntropyRef) - (1 * (reactant_1EntropyRef + (1 * reactant_2EntropyRef)));

            //  delta_G = delta_H - T * delta_S
            double delta_G = delta_H_0reaction - 298.15 * delta_S_0reaction / 1000;
            double expected_deltaG = -190.59582045;
            Assert.AreEqual(expected_deltaG, delta_G,delta);
        }

        [TestMethod]
        public void Test_New_HeatCapacity()
        {
            double Temperature = 1000;
            double Cp = ThermoDynamics.HeatCapacity(Temperature, coefficients, t_expnts);
            Assert.AreEqual(73.676, Cp, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 3.794)]
        [DataRow(498.15, 8.139)]
        [DataRow(598.15, 13.093)]
        [DataRow(698.15, 18.647)]
        [DataRow(798.15, 24.768)]
        [DataRow(898.15, 31.416)]
        [DataRow(998.15, 38.548)]
        [DataRow(1000.00, 38.685)]
        public void Test_HeatCapacity_No_Library(double T, double expected)
        {

            static double GKIntegrate(Func<double, double> f, double a, double b, double targetRelativeError)
            {
                // Gauss nodes and weights (lower-order)
                double[] gaussNodes = { -0.7745966692, 0, 0.7745966692 };
                double[] gaussWeights = { 0.5555555556, 0.8888888889, 0.5555555556 };
                // Kronrod nodes and weights (higher-order)
                double[] kronrodNodes = { -0.8611363116, -0.3399810436, 0.3399810436, 0.8611363116 };
                double[] kronrodWeights = { 0.3478548451, 0.6521451549, 0.6521451549, 0.3478548451 };
                double integral = 0;
                // Combine Gauss and Kronrod contributions
                for (int i = 0; i < gaussNodes.Length; i++)
                {
                    double gaussPoint = 0.5 * (b - a) * gaussNodes[i] + 0.5 * (b + a);
                    double gaussWeight = 0.5 * (b - a) * gaussWeights[i];

                    double kronrodPoint = 0.5 * (b - a) * kronrodNodes[i] + 0.5 * (b + a);
                    double kronrodWeight = 0.5 * (b - a) * kronrodWeights[i];

                    double gaussValue = f(gaussPoint);
                    double kronrodValue = f(kronrodPoint);

                    integral += gaussWeight * gaussValue + kronrodWeight * kronrodValue;
                }
                // Estimate error
                double error = Math.Abs(integral - (kronrodWeights[0] * f(0.5 * (b - a) + 0.5 * (b + a))));
                // Check if error is within tolerance
                if (error < targetRelativeError * Math.Abs(integral))
                    return integral;
                else
                {
                    // Subdivide interval and recursively compute
                    double mid = 0.5 * (a + b);
                    double a_mid = GKIntegrate(f, a, mid, targetRelativeError);
                    double mid_b = GKIntegrate(f, mid, b, targetRelativeError);
                    return a_mid + mid_b;
                }
            }

            double Temp = 298.15;
            Func<double, double> heatCapacity = x => Math.Log(Temp);
            // integration interval [a, b]
            double a = 0;
            double b = 0.1;
            // set the desired reletive error tolerance
            double targetRelativeError = 0.1;
            // compute the integral using Gauss-Kronrod quadrature
            double result = GKIntegrate(heatCapacity, a, b, targetRelativeError);

            Assert.AreEqual(99, 0);

        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 3.794)]
        [DataRow(498.15, 8.139)]
        [DataRow(598.15, 13.093)]
        [DataRow(698.15, 18.647)]
        [DataRow(798.15, 24.768)]
        [DataRow(898.15, 31.416)]
        [DataRow(998.15, 38.548)]
        [DataRow(1000.00, 38.685)]
        public void Test_EnthalpyRefH298(double T, double expected)
        {
            double enthalpy = ThermoDynamics.EnthalpyRefH298(referenceTemp, T, coefficients, t_expnts);
            Assert.AreEqual(expected, enthalpy, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 0.0)]
        [DataRow(398.15, 0.0)]
        [DataRow(498.15, 0.0)]
        [DataRow(598.15, 0.0)]
        [DataRow(698.15, 0.0)]
        [DataRow(798.15, 0.0)]
        [DataRow(898.15, 0.0)]
        [DataRow(998.15, 0.0)]
        [DataRow(1000.0, 0.0)]
        public void Test_MU(double Temperature, double expected)
        {
            // TODO update with refElements.json
            double NG = 1;
            double Pp = 1.0;
            double Enn = 0.1;
            double Enln = Math.Log(Enn / NG);
            double Tm = Math.Log(Pp / Enn);

            List<string> m_formula = ["C", "H"];
            //List<string> m_formula = ["O2"];
            //List<string> m_formula = ["CH4"];
            // CH4 coefficients
            //string searchString = "C";
            //string searchString = "O2";
            //string searchString = "CO2";
            //string searchString = "O";
            //string searchString = "CH4";
            //string searchString = "H2";
            string searchString = "";
            //ICollection<ChemicalFormula> chemicalFormula = null;
            List<double> temperatureRange = [];
            List<double> coefficients = [];
            List<double> integrationConstants = [];
            List<double> t_expnts = [];
            double heatOfFormation = 0.0;
            double refTemperature = 298.15;
            double Cp_JmolK = 0.0;
            double Enthalpy_kJmol = 0.0;
            double ref_entropy = 0.0;
            double Entropy_JmolK = 0.0;
            double Gibbs_H298JmolK = 0.0;
            double MU = 0.0;

            //var referenceCPHS = InputServices.GetDefaultCPHS("Data/Ref_Defaults.json");
            //ICollection<Species> m_species = InputServices.GetNASA("Data/NASApolynomials.json");

            // refCPHS
            // species
            // refElements


            foreach (string formula in m_formula)
            {
                var m_refElement = from element in refElements
                                   where element.ChemicalFormula.First().Symbol == formula
                                   select element;

                chemicalFormula = m_refElement.First().ChemicalFormula;
                temperatureRange = m_refElement.First().DataRecords.ElementAt(0).TemperatureRange;
                coefficients = m_refElement.First().DataRecords.ElementAt(0).Coefficients;
                integrationConstants = m_refElement.First().DataRecords.ElementAt(0).IntegrationConstants;
                t_expnts = m_refElement.First().DataRecords.ElementAt(0).TExponents;

                heatOfFormation = m_refElement.First().HeatOfFormation;
                Cp_JmolK = ThermoDynamics.HeatCapacity(Temperature, coefficients, t_expnts);
                Enthalpy_kJmol = ThermoDynamics.Enthalpy(refTemperature, heatOfFormation, Temperature, coefficients, t_expnts);
                ref_entropy = 0.0;
                Entropy_JmolK = ThermoDynamics.Entropy(refTemperature, ref_entropy, Temperature, coefficients, t_expnts);
                Gibbs_H298JmolK = ThermoDynamics.GibbsRef(refTemperature, ref_entropy, Temperature, coefficients, t_expnts);
                //MU = ThermoDynamics.Calculate_MU(Gibbs_H298JmolK, Temperature, 1);

                MU = MU + Enthalpy_kJmol - Entropy_JmolK + Enln + Tm;

                //        "Species_Name": "CH4",
                //"Molecular_Weight": 16.04246,
                //"Enthalpy": -84.616,
                //"Delta_Enthalpy": -66.626,
                //"Delta_Enthalpy_Ref": -74.6,
                //"CP_Ref": 35.691,
                //"Enthalpy_Ref": 10.016,
                //"Entropy_Ref": 186.371
               double zz =  -66.626 - Temperature * 186.371/1000;
                double deltaG = Enthalpy_kJmol - (Temperature * (Entropy_JmolK / 1000));
                double mx = (Temperature * (Entropy_JmolK / 1000));
                //MU = MU + Enthalpy_kJmol;
                MU += Gibbs_H298JmolK;
            }

            // CO2 reaction == -394.36
            // C = 558.579  
            // O2 = -205.149  == 353.43
            // CO2 = -607.297

            double delta = 0.005;
            Assert.AreEqual(expected, MU, delta);

        }

        [DataTestMethod]
        [DataRow(298.15, 186.371)]
        [DataRow(398.15, 197.312)]
        [DataRow(498.15, 207.023)]
        [DataRow(598.15, 216.068)]
        [DataRow(698.15, 224.642)]
        [DataRow(798.15, 232.828)]
        [DataRow(898.15, 240.669)]
        [DataRow(998.15, 248.195)]
        [DataRow(1000.00, 248.331)]
        public void TestMultipleEntropyConditions(double T, double expected)
        {
            double ref_entropy = (double)m_referenceSpecie.First().Entropy_Ref;
            double result = ThermoDynamics.Entropy(referenceTemp, ref_entropy, T, coefficients, t_expnts);
            Assert.AreEqual(expected, result, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, 186.371)]
        [DataRow(398.15, 187.782)]
        [DataRow(498.15, 190.684)]
        [DataRow(598.15, 194.179)]
        [DataRow(698.15, 197.933)]
        [DataRow(798.15, 201.796)]
        [DataRow(898.15, 205.691)]
        [DataRow(998.15, 209.575)]
        [DataRow(1000.00, 209.646)]
        public void TestGibbs(double T, double expected)
        {
            double ref_entropy = (double)m_referenceSpecie.First().Entropy_Ref;
            double thermoGibbs = ThermoDynamics.GibbsRef(referenceTemp, ref_entropy, T, coefficients, t_expnts);
            Assert.AreEqual(expected, thermoGibbs, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, -74.600)]
        [DataRow(398.15, -70.806)]
        [DataRow(498.15, -66.461)]
        [DataRow(598.15, -61.507)]
        [DataRow(698.15, -55.953)]
        [DataRow(798.15, -49.832)]
        [DataRow(898.15, -43.184)]
        [DataRow(998.15, -36.052)]
        [DataRow(1000.00, -35.915)]
        public void TestThermoEnthalpyMethod(double T, double expected)
        {
            double CH4HeatOfFormation = m_reactant.ElementAt(0).HeatOfFormation;
            double enthalpy = ThermoDynamics.Enthalpy(referenceTemp,CH4HeatOfFormation, T, coefficients, t_expnts);
            Assert.AreEqual(expected, enthalpy, delta);
        }

        [DataTestMethod]
        [DataRow(298.15, -74.600)]
        [DataRow(398.15, -77.635)]
        [DataRow(498.15, -80.457)]
        [DataRow(598.15, -82.932)]
        [DataRow(698.15, -85.023)]
        [DataRow(798.15, -86.726)]
        [DataRow(898.15, -88.059)]
        [DataRow(998.15, -89.053)]
        [DataRow(1000.00, -89.069)]
        public void TestDeltaHf(double T, double expected)
        {
            // CH4 heat of formation = -74600.0
            // delta H_f(T) = H(T) - SUM delta H_f(elements)

            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double delta = 0.005;
            double ref_temp = 298.15;

            double C_298HF = 1053.500;
            double H_298HF = 8468.102;
            double heatOfFormation = 74600.0;

            double elementsSum = C_298HF + H_298HF;

            double enthalpy = ThermoDynamics.Enthalpy(ref_temp,heatOfFormation, T, coefficients, t_expnts);

            double deltaH_f = enthalpy - elementsSum;


            Assert.AreEqual(expected, deltaH_f);

        }

        [TestMethod]
        public void Test_CalculateDeltaHf()
        {

            //            CH4 Gurvich,1991 pt1 p44 pt2 p36.                                 
            // 2 g 8 / 99 C   1.00H   4.00    0.00    0.00    0.00 0   16.0424600 - 74600.000
            //    200.000   1000.0007 - 2.0 - 1.0  0.0  1.0  2.0  3.0  4.0  0.0        10016.202
            //- 1.766850998D + 05 2.786181020D + 03 - 1.202577850D + 01 3.917619290D - 02 - 3.619054430D - 05
            // 2.026853043D - 08 - 4.976705490D - 12 - 2.331314360D + 04 8.904322750D + 01
            //   1000.000   6000.0007 - 2.0 - 1.0  0.0  1.0  2.0  3.0  4.0  0.0        10016.202
            // 3.730042760D + 06 - 1.383501485D + 04 2.049107091D + 01 - 1.961974759D - 03 4.727313040D - 07
            //- 3.728814690D - 11 1.623737207D - 15                 7.532066910D + 04 - 1.219124889D + 02


            double T = 398.15;
            double deltaHf_298 = -74.600;
            double C_formation = 716680.0 * 0;
            double H_formation = 0.0 * 2 * 0;
            double[] elementDeltaHf_298 = { C_formation, H_formation };

            double HH_RT = CalculateH_RT(T);
            double deltaHFF = CalculateDeltaHf(T, deltaHf_298, elementDeltaHf_298);

            Assert.AreEqual(77.635, 0);
        }

        private double CalculateDeltaHf(double t, double deltaHf_298, double[] elementDeltaHf_298)
        {
            double H_RT = CalculateH_RT(t);
            double deltaHf_T = H_RT - deltaHf_298;

            foreach (double elementHf in elementDeltaHf_298)
            {
                deltaHf_T -= elementHf;
            }
            return deltaHf_T;

        }

        private double CalculateH_RT(double t)
        {
            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            double heatOfFormation = -74600.0;
            double ref_enthalpy = heatOfFormation / 1000.0;

            //double ref_temp = 298.15;
            double T = 398.15;
            double a1 = coefficients[0];
            double a2 = coefficients[1];
            double a3 = coefficients[2];
            double a4 = coefficients[3];
            double a5 = coefficients[4];
            double a6 = coefficients[5];
            double a7 = coefficients[6];
            //double a8 = coefficients[7];

            double coef = -a1 * Math.Pow(T, -2)
                            + (a2 * Math.Pow(T, -1) * Math.Log(T))
                            + a3
                            + (a4 * T / 2)
                            + (a5 * Math.Pow(T, 2) / 3)
                            + (a6 * Math.Pow(T, 3) / 4)
                            + (a7 * Math.Pow(T, 4) / 5)
                            + integrationConstants[0] / T;
            return coef * T * 8.314 / 1000;
        }

        [TestMethod]
        public void Test_MU_For_TempRanges()
        {

            List<double> temperatureRange = [200.000, 1000.000];
            List<double> t_expnts = [-2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 0.0];
            List<double> coefficients = [-1.766850998e+05, 2.786181020e+03, -1.202577850e+01, 3.917619290e-02, -3.619054430e-05, 2.026853043e-08, -4.976705490e-12];
            List<double> integrationConstants = [-2.331314360e+04, 8.904322750e+01];

            // Arrange T = 298.15 = -50.72 kj/mol, 398.15 = -43.95 kj/mol, 498.15 = -37.75 kj/mol
            // get the Gibbs for CH4
            double m_Gibbs = ThermoDynamics.GibbsRef(298.15,187.0, 298.15, coefficients, t_expnts);
            double expected_MU = -50.72;
            double temperature = 298.15;
            double pressure = 1.0;

            // Act
            double actual_MU = ThermoDynamics.Calculate_MU(m_Gibbs, temperature, pressure);

            // Assert
            Assert.AreEqual(expected: expected_MU, actual: actual_MU);
        }

    }

    [TestClass]
    public class TestViewModels
    {
        [TestMethod]
        public void TestBaseViewModel()
        {
            var viewModel = new ReactantsViewModel();
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void TestBasePropellantsViewModel()
        {
            var viewModel = new PropellantViewModel();
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        public void AddItem_ShouldIncreasePropellantsCount()
        {
            // Arrange
            ICollection<Reactant> reactants = ThermoService.GetReactants();
            Reactant? searchedItem = reactants?.Where(item => item.Name == "CH4").FirstOrDefault();

            var viewModel = new PropellantViewModel();
            var initialCount = viewModel.ReactantsCollection.Count;

            // Act
            viewModel.AddItem(searchedItem);

            // Assert
            Assert.AreEqual(initialCount + 1, viewModel.ReactantsCollection.Count);

        }

    }

    [TestClass]
    public class TestModifyAddNode
    {
        [TestMethod]
        public void TestAddNode()
        {
            ICollection<Species> reactants = InputServices.GetNASA("Data/NASApolynomials.json");
            int reactantCount = reactants.Count;

            List<Species>? filteredCollection = reactants?.Where(item => item.Name == "CH4").ToList();
            var molecularWeight = (from item in filteredCollection
                                   select item.MolecularWeight).FirstOrDefault();
            double expected = 16.0424600;

            //var elementv = reactants.ElementAt(0);

            //List<DTO_Reactant> dtoList = new();

            //for (int i = 0; i < reactantCount; i++)
            //{
            //    DTO_Reactant dTO_Reactant = new()
            //    {
            //        Molecule = new(),

            //    };

            //    dTO_Reactant.Name = reactants.ElementAt(i).Name;
            //    dTO_Reactant.Description = reactants.ElementAt(i).Description;
            //    dTO_Reactant.T_Intervals = reactants.ElementAt(i).T_Intervals;
            //    dTO_Reactant.Id_Code = reactants.ElementAt(i).Id_Code;
            //    dTO_Reactant.Molecule.Count = 1.0;
            //    dTO_Reactant.Molecule.ChemicalFormula = reactants.ElementAt(i).Molecule.ChemicalFormula;
            //    dTO_Reactant.Gaseous = reactants.ElementAt(i).Gaseous;
            //    dTO_Reactant.MolecularWeight = reactants.ElementAt(i).MolecularWeight;
            //    dTO_Reactant.HeatOfFormation = reactants.ElementAt(i).HeatOfFormation;
            //    dTO_Reactant.TemperatureRange = reactants.ElementAt(i).TemperatureRange;

            //    dtoList.Add(dTO_Reactant);
            //}

            //string serializedList = JsonConvert.SerializeObject(dtoList);
            ////string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newShortThermo.json");
            ////File.WriteAllText(path, serializedList);

            Assert.AreEqual(expected, molecularWeight);

        }
    }
}

