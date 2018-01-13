# HookGit

<p align="center">
  <img src="https://i.imgur.com/YSwhpIe.png"/>
</p>

# General information

HookGit is an inhouse utility that was developed for Tapp Solutions. The repository for this application has been made public for others to get ideas how to implement similar features in their apps.

HookGit provides the following:

- GitHub webhook support

   - Notifying when events occure in a repository such as, but not limited to: Pushed, Commit comments, Issues, Project cards, labels
 
* GitHub access support
   - Supports issue modifications, assigning users, assigning labels
 
* Service check-ups
   - Querying services for information, such as stats and/or uptime
 
* Translation
   - Messages posted in discord in other languages than English will be translated with help of Microsoft Azure translation service
 
* Chatbot
   - Yields simple responses via the Cleverbot API
 
* Discord commands for interactive data lookup and edits

   - **GithubModule**
      - !createissue
      - !closeissue
      - !assignissue
      - !labelissue
      - !listissues
   - **HelpModule**
      - !help
      - !help
   - **ServiceModule**
      - !stats
   - **BasicModule**
      - !roll
      - !8ball

---

Example GitHub webhook relay to Discord

<img src="https://i.imgur.com/PkKQObO.png"/>
