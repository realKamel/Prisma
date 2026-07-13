using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Prisma.Application.Abstractions.Ai;
using Prisma.Application.Common.Constants;
using Prisma.Application.Common.DTOs.Ai;

namespace Prisma.Infrastructure.Ai;

public sealed class ReportGeneratorService(
    [FromKeyedServices(AIAgentRole.ChatAgent.ReportGeneratorAgent)]
    AIAgent agent)
    : IReportGenerator
{
    public async Task<string> GenerateReportAsync(StudentData request, CancellationToken ct)
    {
        var prompt = BuildPrompt(request);

        var response = await agent.RunAsync(prompt, cancellationToken: ct);

        return response.Text;
    }

    private static string BuildPrompt(StudentData message)
    {
        var quizzes = message
            .Attempts
            .Aggregate("", (current, attempt) =>
                string.Join("\n", current, attempt.QuizTitle, attempt.Degree));

        var enrollments = message.Enrollments
            .Aggregate("", (current, enrollment) =>
                string.Join("\n", current, $"{enrollment.EnrollmentId}:{enrollment.IsCompleted}",
                    enrollment.LessonReport));

        return $"""
                You are an expert Data Analyst and Educational Consultant specializing in Learning Management Systems (LMS). 

                I am going to provide you with raw data regarding student performance and engagement within our LMS. Your task is to analyze this data and generate a comprehensive, executive-ready Student Performance Report.

                The data provided is fully sufficient for this task. Please structure the report into the following distinct sections:

                1. Executive Summary: A high-level overview of overall student health, average performance metrics, and standout trends (positive or negative).
                2. Engagement & Progress Analysis: Analyze how students are interacting with the system (e.g., login frequency, lesson completion rates, time spent on modules). Identify where students are thriving and where momentum drops off.
                3. Academic Performance Breakdown: Evaluation of assessment scores, quizzes, and practical assignments. Highlight concepts or modules where students face the highest friction or failure rates.
                4. At-Risk & Top Performers: 
                   - Identify specific patterns or metrics that define an "at-risk" student based on the data, and flag any urgent interventions needed.
                   - Highlight top-performing students or behaviors that correlate with high success.
                5. Actionable Recommendations: Provide 3-5 concrete, data-driven next steps for instructors or system administrators to improve completion rates and material retention.

                Tone & Style Instructions:
                - Keep it professional, analytical, and highly actionable.
                - You Can Use Student Name {message.StudentName} & And this is Student Id if Needed {message.StudentId}
                - Use clear headings, bullet points, and clean formatting so it is easy to read at a glance.
                - Avoid vague statements like "students are doing well." Instead, use concrete data points from the input (e.g., "Module 3 shows a 25% drop in completion rates compared to Module 2").

                Here is the data:
                {quizzes}
                ---
                {enrollments}
                """;
    }
}