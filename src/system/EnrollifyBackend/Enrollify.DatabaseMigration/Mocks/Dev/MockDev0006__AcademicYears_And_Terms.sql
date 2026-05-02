DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');


-- ######### Academic Years ##############

CREATE TABLE #AcademicYears(
    [StartDate] DATE NOT NULL,
    [EndDate]   DATE NOT NULL
);

INSERT INTO #AcademicYears ([StartDate], [EndDate])
VALUES
('2014-08-04', '2015-06-05'),
('2015-08-03', '2016-06-03'),
('2016-08-01', '2017-06-02'),
('2017-08-07', '2018-06-01'),
('2018-08-06', '2019-05-31'),
('2019-08-05', '2020-06-05'),
('2020-08-10', '2021-06-04'),
('2021-08-09', '2022-06-03'),
('2022-08-08', '2023-06-02'),
('2023-08-07', '2024-06-07');

MERGE [AcademicYears] AS [Target]
USING
    (SELECT [StartDate], [EndDate] FROM #AcademicYears) AS [Source]
    ON [Target].[StartDate] = [Source].[StartDate] AND [Target].[EndDate] = [Source].[EndDate]
WHEN MATCHED THEN
    UPDATE SET
        [Target].[UpdatedBy] = @InitialUserId,
        [Target].[UpdatedAt] = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT ([StartDate], [EndDate], [CreatedBy], [CreatedAt])
    VALUES ([Source].[StartDate], [Source].[EndDate], @InitialUserId, GETUTCDATE());


-- ######### Academic Terms ##############

CREATE TABLE #AcademicTerms(
    [TermNumber]          INT  NOT NULL,
    [AcademicYearStart]   DATE NOT NULL, -- used to look up AcademicYearId
    [StartDate]           DATE NOT NULL,
    [EndDate]             DATE NOT NULL
);

--┌──────┬──────────────────────────────────┐
--│ Term │ Approx. Duration                 │
--├──────┼──────────────────────────────────┤
--│ 1st  │ Early Aug → Mid-Nov (~14 weeks)  │
--├──────┼──────────────────────────────────┤
--│ 2nd  │ Late Nov → Early Mar (~15 weeks) │
--├──────┼──────────────────────────────────┤
--│ 3rd  │ Mid-Mar → Early Jun (~11 weeks)  │
--└──────┴──────────────────────────────────┘

INSERT INTO #AcademicTerms ([TermNumber], [AcademicYearStart], [StartDate], [EndDate])
VALUES
-- AY 2014-2015
(1, '2014-08-04', '2014-08-04', '2014-11-14'),
(2, '2014-08-04', '2014-11-24', '2015-03-06'),
(3, '2014-08-04', '2015-03-16', '2015-06-05'),
-- AY 2015-2016
(1, '2015-08-03', '2015-08-03', '2015-11-13'),
(2, '2015-08-03', '2015-11-23', '2016-03-04'),
(3, '2015-08-03', '2016-03-14', '2016-06-03'),
-- AY 2016-2017
(1, '2016-08-01', '2016-08-01', '2016-11-11'),
(2, '2016-08-01', '2016-11-21', '2017-03-03'),
(3, '2016-08-01', '2017-03-13', '2017-06-02'),
-- AY 2017-2018
(1, '2017-08-07', '2017-08-07', '2017-11-17'),
(2, '2017-08-07', '2017-11-27', '2018-03-09'),
(3, '2017-08-07', '2018-03-19', '2018-06-01'),
-- AY 2018-2019
(1, '2018-08-06', '2018-08-06', '2018-11-16'),
(2, '2018-08-06', '2018-11-26', '2019-03-08'),
(3, '2018-08-06', '2019-03-18', '2019-05-31'),
-- AY 2019-2020
(1, '2019-08-05', '2019-08-05', '2019-11-15'),
(2, '2019-08-05', '2019-11-25', '2020-03-06'),
(3, '2019-08-05', '2020-03-16', '2020-06-05'),
-- AY 2020-2021
(1, '2020-08-10', '2020-08-10', '2020-11-20'),
(2, '2020-08-10', '2020-11-30', '2021-03-12'),
(3, '2020-08-10', '2021-03-22', '2021-06-04'),
-- AY 2021-2022
(1, '2021-08-09', '2021-08-09', '2021-11-19'),
(2, '2021-08-09', '2021-11-29', '2022-03-11'),
(3, '2021-08-09', '2022-03-21', '2022-06-03'),
-- AY 2022-2023
(1, '2022-08-08', '2022-08-08', '2022-11-18'),
(2, '2022-08-08', '2022-11-28', '2023-03-10'),
(3, '2022-08-08', '2023-03-20', '2023-06-02'),
-- AY 2023-2024
(1, '2023-08-07', '2023-08-07', '2023-11-17'),
(2, '2023-08-07', '2023-11-27', '2024-03-08'),
(3, '2023-08-07', '2024-03-18', '2024-06-07');

MERGE [AcademicTerms] AS [Target]
USING
    (
        SELECT T.[TermNumber], AY.[Id] AS [AcademicYearId], T.[StartDate], T.[EndDate]
        FROM #AcademicTerms T
        INNER JOIN [AcademicYears] AY ON AY.[StartDate] = T.[AcademicYearStart]
    ) AS [Source]
    ON [Target].[AcademicYearId] = [Source].[AcademicYearId]
    AND [Target].[TermNumber]    = [Source].[TermNumber]
WHEN MATCHED THEN
    UPDATE SET
        [Target].[StartDate]  = [Source].[StartDate],
        [Target].[EndDate]    = [Source].[EndDate],
        [Target].[UpdatedBy]  = @InitialUserId,
        [Target].[UpdatedAt]  = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT ([TermNumber], [AcademicYearId], [StartDate], [EndDate], [CreatedBy], [CreatedAt])
    VALUES ([Source].[TermNumber], [Source].[AcademicYearId], [Source].[StartDate], [Source].[EndDate], @InitialUserId, GETUTCDATE());


DROP TABLE #AcademicYears;
DROP TABLE #AcademicTerms;
