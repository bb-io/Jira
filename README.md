# Blackbird.io Jira

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Jira is a widely used project management and issue tracking tool developed by Atlassian. It provides teams with a platform to plan, track, and manage tasks, projects, and software development processes, helping to streamline collaboration and improve project visibility. This Jira app primarily focuses on issues management.

## Before setting up

Before you can connect you need to make sure that:

- You have an Atlassian account and [Jira site](https://support.atlassian.com/jira-work-management/docs/set-up-your-site/).
- You have a [project created](https://support.atlassian.com/jira-software-cloud/docs/create-a-new-project/).
- You have the right [permissions](https://support.atlassian.com/jira-cloud-administration/docs/permissions-for-company-managed-projects/#Issue-permissions).

### Enable webhooks

If you want to use Jira webhooks, you need to:

- Log in as a user with Administer Jira [global permission](https://support.atlassian.com/jira-cloud-administration/docs/manage-global-permissions/).
- In top right corner choose ![Settings](Images/README/settings.png) > _System_. Under _Advanced_, select _WebHooks_.
- In top right corner choose _Create a WebHook_.
- In _URL_ field specify `https://bridge.blackbird.io/api/webhooks/jira`.
- Make sure that _Status_ is _Enabled_.
- Select everything under _Issue related events_ > _Issue_.
- Scroll to the bottom of the page and click _Create_.

![Selecting events](Images/README/issue_related_events.png)

### Adding custom fields

To create custom fields, follow [this guide](https://confluence.atlassian.com/adminjiraserver/adding-custom-fields-1047552713.html). Once the custom fields you need are created, you need to:

- Choose ![Settings](Images/README/settings.png) > _Projects_ in top right corner.
- For the project you are interested in, select ![More](Images/README/more_button.png) > _Project settings_.
- Select _Issue types_ from the left panel.
- Click on issue type to which you want to add created custom fields.
- Locate _Search all fields_ search bar in the right panel.
- Search for the field you are interested in and drag it to issue's fields.
- Click _Save changes_ button.

Note: this app currently supports only short text (plain text only) custom fields.

## Connecting

1. Navigate to apps and search for Jira. If you cannot find Jira then click _Add App_ in the top right corner, select Jira and add the app to your Blackbird environment.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My organization'.
4. Fill in the base URL to the Jira site you want to connect to. The base URL is of shape `https://<organization name>.atlassian.net`. You can usually copy this part of the URL when you are logged into your Jira instance.
5. Click _Authorize connection_.
6. Follow the instructions that Jira gives you, authorizing Blackbird.io to act on your behalf.
7. When you return to Blackbird, confirm that the connection has appeared and the status is _Connected_.

![Connecting](Images/README/connection.png)

## Actions

Please note: sending too many parallel requests to Jira may result in request rejections. While we have added a retry policy to handle this, ultimate control lies with Jira’s servers.

### Issues

- **Get issue** returns the details for an issue (summary, description, status, priority, assignee, project).
- **Search issues** returns issues that meet the provided criteria. Allows appending custom JQL conditions.
- **List attachments** returns a list of files attached to an issue.
- **Download attachment** returns the contents of an attachment.
- **Create issue**.
- **Add attachment** adds attachment to an issue.
- **Update issue**. Specify only the fields that require updating.
- **Append to issue description** appends text to the description of an issue. Doesn't brake formatting, useful for adding text to the end of the description.
- **Delete issue**.
- **Add labels to issue** adds labels to an issue. Returns the updated issue.
- **Move issues to sprint** Moves issues to a specific sprint
- **Find issue** returns the first issue that matches given conditions. Allows appending custom JQL conditions.

### Issue custom fields

- **Get custom text field value** returns the value of a custom string field (e.g., plain text or URL) for a specific issue.
- **Set custom text field value** sets the value of a custom string field for a specific issue.
- **Get custom dropdown field value** returns the value of a custom dropdown field for a specific issue.
- **Set custom dropdown field value** sets the value of a custom dropdown field for a specific issue.
- **Get custom date field value** returns the value of a custom date field for a specific issue.
- **Set custom date field value** sets the value of a custom date field for a specific issue.
- **Get custom multiselect field values** gets the selected values of a multiple choice select list custom field for a specific issue.
- **Get custom number field value** gets the value of a custom number field
- **Set custom number field value** sets the value of a custom number field
- **Set custom rich text field value** sets the value of a custom rich text field
- **Set custom multiselect field value** sets the value of a custom multiselect field

### User Actions

- **List users**: Lists a predefined number of users (up to 20 by default) from Jira.
- **Get user**: Retrieves detailed information about a specific user identified by their account ID.
- **Delete user**: Deletes a user from Jira based on their account ID. This action is irreversible.
- **Create user**: Creates a new user in Jira with the specified details.
- **Get groups**: Retrieves the groups associated with a specified user.
- **Get user email**: Fetches the email address for a specified user.
- **Get user columns**: Gets the column configuration for a specified user.
- **Reset user default columns**: Resets the column configuration to default for a specified user.
- **Set user columns**: Sets the column configuration for a specified user.
- **Bulk get users**: Retrieves information for a bulk list of users based on their account IDs.
- **Find user by email** Finds a user in Jira by their email address.

### Issue Comment Actions

- **Get sssue comments**: Fetches comments for a specified issue. It can retrieve comments for multiple issues if IDs are provided.
- **Get issue comment**: Retrieves a specific comment from a specified issue.
- **Delete issue comment**: Deletes a comment from a specified issue.
- **Add issue comment**: Adds a new comment to a specified issue.
- **Append text to comment**: Append text to comment on a specified issue.
- **Find issue comment by text**: Find the first comment in an issue that contains the specified text.

### Sprint Actions

- **Get relevant sprint for date** get Sprint corresponding to the specified date for a selected board


### User Properties Actions

- **Get user properties**: Retrieves all properties for a specified user. This can include a variety of user-specific settings and data stored in Jira.
- **Get boolean user property**: Fetches the value of a specific boolean property for a given user. Useful for true/false user settings.
- **Get string user property**: Retrieves the value of a specific string property for a specified user. This can include text-based user settings or information.
- **Get integer user property**: Gets the value of a specific integer property for a user. Ideal for numeric user settings such as limits or thresholds.
- **Get dateuser property**: Fetches the value of a specific date property for a user. Useful for date-based settings or milestones related to the user.
- **GetArray user property**: Retrieves the values of a specific array property for a user. This can be used for lists of user settings or preferences that are stored as arrays.
- **Set user property**: Sets the value of a specified property for a user. This method allows for the dynamic updating of user properties based on provided input.
- **Delete user property**: Deletes a specified property for a user. This action removes the selected property from the user's profile in Jira.

## Events

- **On issue updated** is triggered when an issue is updated. If you want a bird to be triggered when specific issue is updated, specify the issue parameter. Otherwise, you can specify project parameter if you are interested in specific project's issues.
- **On issue created** is triggered when an issue is created. If you want a bird to be triggered when an issue is created in specific project, fill the project parameter.
- **On issue assigned** is triggered when an issue is assigned to specific user. You can specify project parameter if you're interested in specific project.
- **On issue with specific type created** is triggered when an issue created has specific type (for example, a bug) or an existing issue was updated to have specific type. Optionally, you can specify project parameter.
- **On issue with specific priority created** is triggered when an issue created has the specified priority or an existing issue was updated to have the specified priority.
- **On issue deleted** is triggered when an issue is deleted. If you want a bird to be triggered when an issue is deleted in specific project, fill the project parameter.
- **On file attached to issue** is triggered when a file is attached to an issue. If you want a bird to be triggered when a file is attached to specific issue, specify the issue parameter. Otherwise, you can specify project parameter if you are interested in specific project's issues.
- **On issue status changed** is triggered when issue status is changed. If you want a bird to be triggered when specific issue's status is changed, specify the issue parameter. Otherwise, you can specify project parameter if you are interested in specific project's issues. You can also limit the event to trigger only on certain statusses.
- **On issues reach status** is triggered when issueі reaches one of specific statuses.

## Optional inputs

We may provide two options for inputting one property to filter results:

1. **Manual**:
    - **Description**: Allows the user to manually enter value without the assistance of a dropdown list.
    - **Use Case**: This option is ideal when the user knows the exact value they want to use or when the dropdown may not be effective due to the large number of values available through the API.

2. **Dropdown selection**:
    - **Description**: Allows the user to select a value from a dropdown list.
    - **Use Case**: This option is suitable when there are fewer labels, making it convenient for users to select from a predefined list. However, this approach can be less effective if the number of labels is very large, potentially leading to timeout errors during data retrieval.

### Why do we use this approach?

This approach ensures flexibility by providing two methods for inputs, accommodating both situations where the user may need to manually enter specific values and where selecting from a predefined list is more practical. The 'manual' option is particularly useful when dealing with a large number of labels, which could otherwise lead to performance issues when retrieving values from the API.

## Example

![example](Images/README/example.png)

This example bird fetches newest issues and assigns those with highest priority to a specific assignee.

![example_2](image/README/1708600402619.png)

This example shows how to create new TMS (Phrase) projects from issues.

## Missing features

In the future we can add actions for:

- Projects
- Dashboards

Let us know if you're interested!

<!-- end docs -->
