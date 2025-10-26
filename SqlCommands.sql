


CREATE TABLE IF NOT EXISTS `Roles` (
  `Id`   INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Roles_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE IF NOT EXISTS `Users` (
  `Id`       INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(255) NOT NULL,
  `Lastname` VARCHAR(255) NOT NULL,
  `Password` VARCHAR(255) NOT NULL,
  `RoleId`   INT NOT NULL,
  PRIMARY KEY (`Id`),

  UNIQUE KEY `UX_Users_Username` (`Username`),
  KEY `IX_Users_RoleId` (`RoleId`),

  CONSTRAINT `FK_Users_Roles_RoleId`
    FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE IF NOT EXISTS `Companies` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(200) NOT NULL,
    `Street` VARCHAR(200) NULL,
    `City` VARCHAR(100) NULL,
    `Postcode` VARCHAR(20) NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB
DEFAULT CHARSET=utf8mb4
COLLATE=utf8mb4_general_ci;


CREATE TABLE IF NOT EXISTS `Goods` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(255) NOT NULL,
  `Price` DECIMAL(10,2) NOT NULL,
  `ProductNum` VARCHAR(100) NULL,

  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Goods_ProductNum` (`ProductNum`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE IF NOT EXISTS `Warehouses` (
  `Id`        INT NOT NULL AUTO_INCREMENT,
  `Name`      VARCHAR(200) NOT NULL,
  `CompanyId` INT NOT NULL,

  PRIMARY KEY (`Id`),
  KEY `IX_Warehouses_CompanyId` (`CompanyId`),

  UNIQUE KEY `UX_Warehouses_CompanyId_Name` (`CompanyId`,`Name`),

  CONSTRAINT `FK_Warehouses_Company_CompanyId`
    FOREIGN KEY (`CompanyId`) REFERENCES `Companies`(`Id`)
    ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;



INSERT INTO `Roles` (`Name`) VALUES ('Admin'), ('User') ('Moderator');

INSERT INTO `Companies` (`Name`, `Street`, `City`, `Postcode`) VALUES
('TechSolutions s.r.o.', 'Nádražní 45', 'Brno', '602 00'),
('Elektro Novák a syn', 'Hlavní 128', 'Praha', '110 00'),
('Alfa Software CZ a.s.', 'Třída Tomáše Bati 210', 'Zlín', '760 01'),
('Kovoplast s.r.o.', 'U Průmyslovky 32', 'Ostrava', '708 00'),
('GreenMarket CZ s.r.o.', 'Jiráskova 18', 'Plzeň', '301 00');

INSERT INTO `Goods` (`Name`, `Price`, `ProductNum`) VALUES
('zbozi1', 14990.00, 'zb1'),
('zbozi2', 399.00,   'zb2'),
('zbozi3', 1299.00,  'zb3'),
('zbozi4', 5490.00,  'zb4'),
('zbozi5', 890.00,   'zb5');

INSERT INTO `Warehouses` (`Name`, `CompanyId`) VALUES
('Hlavní sklad Praha', 1),
('Expediční sklad Brno', 1),
('Sklad Ostrava – Sever', 2),
('Sklad Zlín', 2),
('Regionální sklad Plzeň', 3),
('Konsignační sklad Hradec', 3);
