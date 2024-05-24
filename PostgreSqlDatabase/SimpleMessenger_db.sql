--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3
-- Dumped by pg_dump version 16.0

-- Started on 2024-05-23 05:46:09

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 218 (class 1259 OID 16408)
-- Name: TextMessages; Type: TABLE; Schema: public; Owner: simplemessenger_user
--

CREATE TABLE public.TextMessages (
    Id bigint NOT NULL,
    Number integer NOT NULL,
    TimeStamp timestamp with time zone NOT NULL,
    Text text NOT NULL,
    SenderId bigint NOT NULL,
    ReceiverId bigint NOT NULL
);


ALTER TABLE public.TextMessages OWNER TO simplemessenger_user;

--
-- TOC entry 217 (class 1259 OID 16407)
-- Name: TextMessages_Id_seq; Type: SEQUENCE; Schema: public; Owner: simplemessenger_user
--

ALTER TABLE public.TextMessages ALTER COLUMN Id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.TextMessages_Id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 216 (class 1259 OID 16400)
-- Name: Users; Type: TABLE; Schema: public; Owner: simplemessenger_user
--

CREATE TABLE public.Users (
    Id bigint NOT NULL,
    Name text NOT NULL
);


ALTER TABLE public.Users OWNER TO simplemessenger_user;

--
-- TOC entry 215 (class 1259 OID 16399)
-- Name: Users_Id_seq; Type: SEQUENCE; Schema: public; Owner: simplemessenger_user
--

ALTER TABLE public.Users ALTER COLUMN Id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.Users_Id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 4896 (class 0 OID 16408)
-- Dependencies: 218
-- Data for Name: TextMessages; Type: TABLE DATA; Schema: public; Owner: simplemessenger_user
--

COPY public.TextMessages (Id, Number, TimeStamp, Text, SenderId, ReceiverId) FROM stdin;
\.


--
-- TOC entry 4894 (class 0 OID 16400)
-- Dependencies: 216
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: simplemessenger_user
--

COPY public.Users (Id, Name) FROM stdin;
1	User_1
2	User_2
3	User_3
\.


--
-- TOC entry 4902 (class 0 OID 0)
-- Dependencies: 217
-- Name: TextMessages_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: simplemessenger_user
--

SELECT pg_catalog.setval('public.TextMessages_Id_seq', 1, false);


--
-- TOC entry 4903 (class 0 OID 0)
-- Dependencies: 215
-- Name: Users_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: simplemessenger_user
--

SELECT pg_catalog.setval('public.Users_Id_seq', 3, true);


--
-- TOC entry 4747 (class 2606 OID 16414)
-- Name: TextMessages PK_TextMessages; Type: CONSTRAINT; Schema: public; Owner: simplemessenger_user
--

ALTER TABLE ONLY public.TextMessages
    ADD CONSTRAINT PK_TextMessages PRIMARY KEY (Id);


--
-- TOC entry 4742 (class 2606 OID 16406)
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: simplemessenger_user
--

ALTER TABLE ONLY public.Users
    ADD CONSTRAINT PK_Users PRIMARY KEY (Id);


--
-- TOC entry 4743 (class 1259 OID 16425)
-- Name: IX_TextMessages_Number_SenderId; Type: INDEX; Schema: public; Owner: simplemessenger_user
--

CREATE UNIQUE INDEX IX_TextMessages_Number_SenderId ON public.TextMessages USING btree (Number, SenderId);


--
-- TOC entry 4744 (class 1259 OID 16426)
-- Name: IX_TextMessages_ReceiverId; Type: INDEX; Schema: public; Owner: simplemessenger_user
--

CREATE INDEX IX_TextMessages_ReceiverId ON public.TextMessages USING btree (ReceiverId);


--
-- TOC entry 4745 (class 1259 OID 16427)
-- Name: IX_TextMessages_SenderId; Type: INDEX; Schema: public; Owner: simplemessenger_user
--

CREATE INDEX IX_TextMessages_SenderId ON public.TextMessages USING btree (SenderId);


--
-- TOC entry 4740 (class 1259 OID 16428)
-- Name: IX_Users_Name; Type: INDEX; Schema: public; Owner: simplemessenger_user
--

CREATE UNIQUE INDEX IX_Users_Name ON public.Users USING btree (Name);


--
-- TOC entry 4748 (class 2606 OID 16415)
-- Name: TextMessages FK_TextMessages_Users_ReceiverId; Type: FK CONSTRAINT; Schema: public; Owner: simplemessenger_user
--

ALTER TABLE ONLY public.TextMessages
    ADD CONSTRAINT FK_TextMessages_Users_ReceiverId FOREIGN KEY (ReceiverId) REFERENCES public.Users(Id) ON DELETE CASCADE;


--
-- TOC entry 4749 (class 2606 OID 16420)
-- Name: TextMessages FK_TextMessages_Users_SenderId; Type: FK CONSTRAINT; Schema: public; Owner: simplemessenger_user
--

ALTER TABLE ONLY public.TextMessages
    ADD CONSTRAINT FK_TextMessages_Users_SenderId FOREIGN KEY (SenderId) REFERENCES public.Users(Id) ON DELETE CASCADE;


-- Completed on 2024-05-23 05:46:10

--
-- PostgreSQL database dump complete
--

