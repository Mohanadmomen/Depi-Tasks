CREATE TABLE Department (
    Dnum INT PRIMARY KEY,
    Dname VARCHAR(50) NOT NULL UNIQUE,
    Location VARCHAR(50),
    HiringDate DATE,
    ESSN INT      
);


CREATE TABLE Employee (
    SSN INT PRIMARY KEY,
    BirthDate DATE,
    Gender CHAR(1) CHECK (Gender IN ('M','F')),
    Fname VARCHAR(30) NOT NULL,
    Lname VARCHAR(30) NOT NULL,
    Dnum INT NULL,
    Supervisor INT NULL,

    CONSTRAINT FK_Employee_Department
        FOREIGN KEY (Dnum)
        REFERENCES Department(Dnum)
        ON DELETE SET NULL,

    CONSTRAINT FK_Employee_Supervisor
        FOREIGN KEY (Supervisor)
        REFERENCES Employee(SSN)
        ON DELETE NO ACTION
);

CREATE TABLE Project (
    Pnum INT PRIMARY KEY,
    Pname VARCHAR(50) NOT NULL,
    City VARCHAR(50),
    SSN INT,

    CONSTRAINT fk_proj_emp
        FOREIGN KEY (SSN)
        REFERENCES Employee(SSN)
        ON DELETE SET NULL
);

CREATE TABLE Dependents (
    Name VARCHAR(50),
    BirthDate DATE,
    Gender CHAR(1) CHECK (Gender IN ('M','F')),
    SSN INT,

    PRIMARY KEY (Name, SSN),

    CONSTRAINT fk_dep_emp
        FOREIGN KEY (SSN)
        REFERENCES Employee(SSN)
        ON DELETE CASCADE
);

CREATE TABLE Employee_Project (
    SSN INT,
    Pnum INT,
    WorkingHours INT CHECK (WorkingHours >= 0),

    PRIMARY KEY (SSN, Pnum),

    FOREIGN KEY (SSN)
        REFERENCES Employee(SSN)
        ON DELETE CASCADE,

    FOREIGN KEY (Pnum)
        REFERENCES Project(Pnum)
        ON DELETE CASCADE
);



