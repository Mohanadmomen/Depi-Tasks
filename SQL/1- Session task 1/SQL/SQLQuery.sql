
--Insert sample data into DEPARTMENT table (at least 3 departments)

INSERT INTO Department (Dnum, Dname, Location, HiringDate, ESSN)
VALUES
(1, 'IT', 'Cairo', '2020-01-01', NULL),
(2, 'HR', 'Giza', '2021-03-15', NULL),
(3, 'Finance', 'Alexandria', '2019-06-10', NULL);


--Insert sample data into EMPLOYEE table (at least 5 employees)

INSERT INTO Employee (SSN, BirthDate, Gender, Fname, Lname, Dnum, Supervisor)
VALUES
('1000', '1980-01-01', 'M', 'Hassan', 'Manager', 1, NULL),
('1003', '1979-08-12', 'M', 'Omar', 'Youssef', 2, NULL),
('1004', '1982-01-30', 'F', 'Mona', 'Adel', 3, NULL),
('1001', '1998-02-10', 'M', 'Ahmed', 'Ali', 1, '1000'),
('1002', '1999-05-21', 'F', 'Sara', 'Hassan', 1, '1000');

--Update an employee's department

UPDATE Department SET ESSN = '1000' WHERE Dnum = 1;
UPDATE Department SET ESSN = '1003' WHERE Dnum = 2;
UPDATE Department SET ESSN = '1004' WHERE Dnum = 3;

--Update an employee's department

UPDATE Employee
SET Dnum = 2
WHERE SSN = '1002';

--Delete a dependent record

INSERT INTO Dependents (Name,BirthDate,Gender,SSN)
VALUES
('Mohanad', '2004-01-02', 'M', '1001');



DELETE FROM Dependents
WHERE Name = 'Mohanad' AND SSN = '1001';


--Retrieve all employees working in a specific department
SELECT 
    E.SSN,
    E.Fname,
    E.Lname,
    D.Dname
FROM Employee E
JOIN Department D ON E.Dnum = D.Dnum
WHERE D.Dname = 'IT';

--Find all employees and their project assignments with working hours
INSERT INTO Project (Pnum, Pname, City, SSN)
VALUES
(10, 'Website System', 'Cairo', '1001'),
(20, 'HR Portal', 'Giza', '1003'),
(30, 'Finance Audit', 'Alexandria', '1004');

INSERT INTO Employee_Project (SSN, Pnum, WorkingHours)
VALUES
('1001', 10, 20),
('1002', 10, 15),
('1003', 20, 25),
('1004', 30, 30),
('1001', 30, 10);

SELECT
    E.Fname,
    E.Lname,
    P.Pname,
    EP.WorkingHours
FROM Employee E
JOIN Employee_Project EP ON E.SSN = EP.SSN
JOIN Project P ON EP.Pnum = P.Pnum;


--session 3 Task 1



--Deleting an employee should delete their dependents automatically

INSERT INTO Employee (SSN, BirthDate, Gender, Fname, Lname, Dnum, Supervisor)
VALUES (1999, '2004-06-15', 'M', 'Ahmed', 'Mostafa', 1, 1001)

INSERT INTO Dependents (Name, BirthDate, Gender, SSN)
VALUES 
('Sara', '2018-04-10', 'F', 1999),
('Omar', '2020-09-22', 'M', 1999);

DELETE FROM Employee WHERE SSN = 1999;



