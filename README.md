# <img src="https://activemq.apache.org/assets/img/activemq_logo_icon_border.png" alt="logo" style="width:128px;"/> Apache.NMS.RestAPI


__Unofficial__ REST API to Apache NMS

## Goals

The Apache NMS Api already offers really good client libraries and in ActiveMQ Artemis a wonderful [REST interface](https://activemq.apache.org/components/artemis/documentation/1.0.0/rest.html).

This project share the same goals.

>## Goals of REST Interface
>
>Why would you want to use Apache ActiveMQ Artemis's REST interface? What are the goals of the REST interface?
>- Easily usable by machine-based (code) clients.
>- Zero client footprint. We want Apache ActiveMQ Artemis to be usable by any client/programming language that has an adequate HTTP client library. You shouldn't have to download, install, and configure a special library to interact with Apache ActiveMQ Artemis.
>- Lightweight interoperability. The HTTP protocol is strong enough to be our message exchange protocol. Since interactions are RESTful the HTTP uniform interface provides all the interoperability you need to communicate between different languages, platforms, and even messaging implementations that choose to implement the same RESTful interface as Apache ActiveMQ Artemis (i.e. the REST-* effort.)
>- No envelope (e.g. SOAP) or feed (e.g. Atom) format requirements. You shouldn't have to learn, use, or parse a specific XML document format in order to send and receive messages through Apache ActiveMQ Artemis's REST interface.
>- Leverage the reliability, scalability, and clustering features of Apache ActiveMQ Artemis on the back end without sacrificing the simplicity of a REST interface.

## Ok, but why?

The REST interface of Artemis unlocks a huge power. But somethimes you don't want to expose such an interface.

This project aims to help you in case:

- You need a **simple** REST API - e.g.: whenever you would like to avoid any dependency on a Apache.NMS Client
- You need a **boilerplate** or a **middleware** for/between your applications
- You are unable to use the native REST Interface of Artemis - e.g.: your environment has a different message bus like JMS or EMS, but you could use the Apache.NMS library

## How to use

`todo`
